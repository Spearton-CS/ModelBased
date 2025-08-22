using System.Collections;
using System.Runtime.CompilerServices;

namespace ModelBased.Collections.Generic
{
    using ComponentModel;

    public class ModelPool<TModel, TID> : IModelPool<ModelPool<TModel, TID>, TModel, TID>
        where TID : notnull
        where TModel : IDataModel<TModel, TID>
    {
        protected IPoolActiveStack<TModel, TID> activeStack = [];
        protected IPoolShadowStack<TModel, TID> shadowStack;
        protected SemaphoreSlim semaphore = new(1, 1);

        protected ModelPool() { }
        public ModelPool(int shadowStackCapacity = 20)
        {
            shadowStack = new PoolShadowStack<TModel, TID>(shadowStackCapacity);
        }

        #region Properties

        /// <inheritdoc/>
        public static ModelPool<TModel, TID> Shared { get; } = new();

        /// <inheritdoc/>
        public virtual int ShadowCapacity => shadowStack.Capacity;

        /// <inheritdoc/>
        public virtual int ShadowCount => shadowStack.Count;

        /// <inheritdoc/>
        public virtual int Count => activeStack.Count;

        #endregion

        #region Clear shadow

        /// <inheritdoc/>
        public virtual int ClearShadow(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return shadowStack.Clear(token);
        }

        /// <inheritdoc/>
        public virtual async Task<int> ClearShadowAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await shadowStack.ClearAsync(token);
        }

        #endregion

        #region Searching

        /// <inheritdoc/>
        public virtual bool Contains(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return activeStack.Contains(id, token) || shadowStack.Contains(id, token);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await activeStack.ContainsAsync(id, token) || await shadowStack.ContainsAsync(id, token);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return from x in activeStack.ContainsMany(ids, token).Zip(shadowStack.ContainsMany(ids, token))
                   select x.First | x.Second;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await using IAsyncEnumerator<bool> active = activeStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token),
                shadow = shadowStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token);

            while (await active.MoveNextAsync() && await shadow.MoveNextAsync())
                yield return active.Current || shadow.Current;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await using IAsyncEnumerator<bool> active = activeStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token),
                shadow = shadowStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token);

            while (await active.MoveNextAsync() && await shadow.MoveNextAsync())
                yield return active.Current || shadow.Current;
        }

        /// <inheritdoc/>
        public virtual bool IsRented(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return activeStack.Contains(id, token);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> IsRentedAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await activeStack.ContainsAsync(id, token);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return activeStack.ContainsMany(ids, token);
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> IsRentedManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (bool value in activeStack.ContainsManyAsync(ids, token))
                yield return value;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> IsRentedManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (bool value in activeStack.ContainsManyAsync(ids, token))
                yield return value;
        }

        #endregion

        #region Modify

        #region Single

        /// <inheritdoc/>
        public virtual bool Modify<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            var (_, Model) = activeStack.TryRef(id, token);
            if (Model is not null)
            {
                try
                {
                    if (Model is TUpdateableModel upd)
                        upd.Update(mod, token);
                    else
                        throw new InvalidCastException($"Model with id '{id}' isn't {nameof(TUpdateableModel)}");
                }
                finally
                {
                    activeStack.TryUnref(Model, default);
                }

                return true; //Work only when we don't have the exception
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public virtual bool Modify<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            int refs = activeStack.TryRef(src, token);
            if (refs > 0)
            {
                try
                {
                    src.Update(mod, token);
                }
                finally
                {
                    activeStack.TryUnref(src, default);
                }

                return true; //Work only when we don't have the exception
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsync<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            var (_, Model) = await activeStack.TryRefAsync(id, token);
            if (Model is not null)
            {
                try
                {
                    if (Model is TUpdateableModel upd)
                        upd.Update(mod, token);
                    else
                        throw new InvalidCastException($"Model with id '{id}' isn't {nameof(TUpdateableModel)}");
                }
                finally
                {
                    activeStack.TryUnref(Model, default);
                }

                return true; //Work only when we don't have the exception
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsyncA<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            var (_, Model) = await activeStack.TryRefAsync(id, token);
            if (Model is not null)
            {
                try
                {
                    if (Model is TUpdateableModel upd)
                        await upd.UpdateAsync(mod, token);
                    else
                        throw new InvalidCastException($"Model with id '{id}' isn't {nameof(TUpdateableModel)}");
                }
                finally
                {
                    activeStack.TryUnref(Model, default);
                }

                return true; //Work only when we don't have the exception
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsync<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            int refs = await activeStack.TryRefAsync(src, token);
            if (refs > 0)
            {
                try
                {
                    src.Update(mod, token);
                }
                finally
                {
                    activeStack.TryUnref(src, default);
                }

                return true; //Work only when we don't have the exception
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsyncA<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            int refs = await activeStack.TryRefAsync(src, token);
            if (refs > 0)
            {
                try
                {
                    await src.UpdateAsync(mod, token);
                }
                finally
                {
                    activeStack.TryUnref(src, default);
                }

                return true; //Work only when we don't have the exception
            }
            else
                return false;
        }

        #endregion

        #region Many

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ModifyMany<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods)
            {
                var (_, Model) = activeStack.TryRef(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            upd.Update(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        activeStack.TryUnref(Model, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ModifyMany<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods)
            {
                int refs = activeStack.TryRef(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        srcWithMod.Src.Update(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        activeStack.TryUnref(srcWithMod.Src, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods)
            {
                var (_, Model) = await activeStack.TryRefAsync(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            upd.Update(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods.WithCancellation(token))
            {
                var (_, Model) = await activeStack.TryRefAsync(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            upd.Update(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods)
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        srcWithMod.Src.Update(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods.WithCancellation(token))
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        srcWithMod.Src.Update(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods)
            {
                var (_, Model) = await activeStack.TryRefAsync(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            await upd.UpdateAsync(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods.WithCancellation(token))
            {
                var (_, Model) = await activeStack.TryRefAsync(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            await upd.UpdateAsync(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods)
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        await srcWithMod.Src.UpdateAsync(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods.WithCancellation(token))
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        await srcWithMod.Src.UpdateAsync(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }

                    yield return true;
                }
                else
                    yield return false;

                token.ThrowIfCancellationRequested();
            }
        }

        #endregion

        #region Many ignore

        /// <inheritdoc/>
        public virtual bool ModifyManyIgnore<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods)
            {
                var (_, Model) = activeStack.TryRef(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            upd.Update(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        activeStack.TryUnref(Model, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual bool ModifyManyIgnore<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods)
            {
                int refs = activeStack.TryRef(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        srcWithMod.Src.Update(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        activeStack.TryUnref(srcWithMod.Src, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods)
            {
                var (_, Model) = await activeStack.TryRefAsync(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            upd.Update(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            await foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods.WithCancellation(token))
            {
                var (_, Model) = activeStack.TryRef(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            upd.Update(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods)
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        srcWithMod.Src.Update(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            await foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods.WithCancellation(token))
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        srcWithMod.Src.Update(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods)
            {
                var (_, Model) = await activeStack.TryRefAsync(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            await upd.UpdateAsync(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            await foreach ((TID ID, TUpdateableModel Mod) idWithMod in idWithMods.WithCancellation(token))
            {
                var (_, Model) = await activeStack.TryRefAsync(idWithMod.ID, token);
                if (Model is not null)
                {
                    try
                    {
                        if (Model is TUpdateableModel upd)
                            await upd.UpdateAsync(idWithMod.Mod, token);
                        else
                            throw new InvalidCastException($"Model with id '{idWithMod.ID}' isn't {nameof(TUpdateableModel)}");
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(Model, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods)
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        await srcWithMod.Src.UpdateAsync(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            bool result = true;
            await foreach ((TUpdateableModel Src, TUpdateableModel Mod) srcWithMod in srcWithMods.WithCancellation(token))
            {
                int refs = await activeStack.TryRefAsync(srcWithMod.Src, token);
                if (refs > 0)
                {
                    try
                    {
                        await srcWithMod.Src.UpdateAsync(srcWithMod.Mod, token);
                    }
                    finally
                    {
                        await activeStack.TryUnrefAsync(srcWithMod.Src, default);
                    }
                }
                else
                    result = false;

                token.ThrowIfCancellationRequested();
            }

            return result;
        }

        #endregion

        #endregion

        #region Rent/Return

        /// <inheritdoc/>
        public virtual (TModel Model, int Refs) Rent(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                TModel? model = shadowStack.TryPop(id, token);
                if (model is not null)
                    return (model, activeStack.Add(model, 1, token));
                else
                {
                    model = TModel.Factory(id, token);
                    return (model, activeStack.Add(model, 1, token));
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<(TModel Model, int Refs)> RentAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                TModel? model = await shadowStack.TryPopAsync(id, token);
                if (model is not null)
                    return (model, await activeStack.AddAsync(model, 1, token));
                else
                {
                    model = TModel.Factory(id, token);
                    return (model, await activeStack.AddAsync(model, 1, token));
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(TModel Model, int Refs)> RentMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (TID id in ids) //In enumeration here we use pattern -> get lazy -> lock -> do work -> repeat
            {
                semaphore.Wait(token);
                try
                {
                    TModel? model = shadowStack.TryPop(id, token);
                    if (model is not null)
                        yield return (model, activeStack.Add(model, 1, token));
                    else
                    {
                        model = TModel.Factory(id, token);
                        yield return (model, activeStack.Add(model, 1, token));
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(TModel Model, int Refs)> RentManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (TID id in ids)
            {
                await semaphore.WaitAsync(token);
                try
                {
                    TModel? model = await shadowStack.TryPopAsync(id, token);
                    if (model is not null)
                        yield return (model, await activeStack.AddAsync(model, 1, token));
                    else
                    {
                        model = TModel.Factory(id, token);
                        yield return (model, await activeStack.AddAsync(model, 1, token));
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(TModel Model, int Refs)> RentManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (TID id in ids.WithCancellation(token))
            {
                await semaphore.WaitAsync(token);
                try
                {
                    TModel? model = await shadowStack.TryPopAsync(id, token);
                    if (model is not null)
                        yield return (model, await activeStack.AddAsync(model, 1, token));
                    else
                    {
                        model = TModel.Factory(id, token);
                        yield return (model, await activeStack.AddAsync(model, 1, token));
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual int TryReturn(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                int refs = activeStack.TryUnref(model, token);
                if (refs == 0)
                    shadowStack.Push(model, token);

                return refs;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> TryReturnAsync(TModel model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                int refs = await activeStack.TryUnrefAsync(model, token);
                if (refs == 0)
                    await shadowStack.PushAsync(model, token);

                return refs;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> TryReturnMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (TModel model in models)
            {
                semaphore.Wait(token);
                try
                {
                    int refs = activeStack.TryUnref(model, token);
                    if (refs == 0)
                        shadowStack.Push(model, token);

                    yield return refs;
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> TryReturnManyAsync(IEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (TModel model in models)
            {
                await semaphore.WaitAsync(token);
                try
                {
                    int refs = await activeStack.TryUnrefAsync(model, token);
                    if (refs == 0)
                        await shadowStack.PushAsync(model, token);

                    yield return refs;
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> TryReturnManyAsync(IAsyncEnumerable<TModel> models, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (TModel model in models.WithCancellation(token))
            {
                await semaphore.WaitAsync(token);
                try
                {
                    int refs = await activeStack.TryUnrefAsync(model, token);
                    if (refs == 0)
                        await shadowStack.PushAsync(model, token);

                    yield return refs;
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool TryReturnManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool removed = true;
            foreach (TModel model in models)
            {
                semaphore.Wait(token);
                try
                {
                    int refs = activeStack.TryUnref(model, token);
                    if (refs == 0)
                        shadowStack.Push(model, token);
                    else if (refs < 0)
                        removed = false;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            return removed;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryReturnManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool removed = true;
            foreach (TModel model in models)
            {
                await semaphore.WaitAsync(token);
                try
                {
                    int refs = await activeStack.TryUnrefAsync(model, token);
                    if (refs == 0)
                        await shadowStack.PushAsync(model, token);
                    else if (refs < 0)
                        removed = false;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            return removed;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryReturnManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool removed = true;
            await foreach (TModel model in models.WithCancellation(token))
            {
                await semaphore.WaitAsync(token);
                try
                {
                    int refs = await activeStack.TryUnrefAsync(model, token);
                    if (refs == 0)
                        await shadowStack.PushAsync(model, token);
                    else if (refs < 0)
                        removed = false;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            return removed;
        }

        #endregion

        #region Ref/unref

        /// <inheritdoc/>
        public virtual int Subscribe(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual int Subscribe(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<int> SubscribeAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<int> SubscribeAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> SubscribeMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> SubscribeMany(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool SubscribeManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool SubscribeManyIgnore(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SubscribeManyIgnoreAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SubscribeManyIgnoreAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SubscribeManyIgnoreAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SubscribeManyIgnoreAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual int Desubscribe(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual int Desubscribe(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<int> DesubscribeAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<int> DesubscribeAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> DesubscribeMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<int> DesubscribeMany(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool DesubscribeManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool DesubscribeManyIgnore(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            yield break; //We must use it, bc compiler don't allow this method without yield
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DesubscribeManyIgnoreAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Enumeration

        /// <inheritdoc/>
        public virtual IEnumerator<TID> EnumerateIDs(CancellationToken token = default)
        {
            using var en = activeStack.GetEnumerator(token);
            while (en.MoveNext())
                yield return en.Current.ID;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerator<TID> EnumerateIDsAsync(CancellationToken token = default)
        {
            await using var en = activeStack.GetAsyncEnumerator(token);
            while (await en.MoveNextAsync())
                yield return en.Current.ID;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerator<TModel> GetAsyncEnumerator(CancellationToken token = default)
        {
            await using var en = activeStack.GetAsyncEnumerator(token);
            while (await en.MoveNextAsync())
                yield return en.Current;
        }

        public virtual IEnumerator<TModel> GetEnumerator(CancellationToken token)
        {
            using var en = activeStack.GetEnumerator(token);
            while (en.MoveNext())
                yield return en.Current;
        }

        /// <inheritdoc/>
        public virtual IEnumerator<TModel> GetEnumerator() => GetEnumerator(default);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}