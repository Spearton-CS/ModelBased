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
        protected SemaphoreSlim semaphore = new(1, 1); //We must do other logic, where we don't use this semaphore, only use active and shadow stack
        protected ulong version = 0;
        
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
            semaphore.Wait(token);
            try
            {
                return shadowStack.Clear(token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> ClearShadowAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return await shadowStack.ClearAsync(token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Searching

        /// <inheritdoc/>
        public virtual bool Contains(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return activeStack.Contains(id, token) || shadowStack.Contains(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ContainsAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return await activeStack.ContainsAsync(id, token) || await shadowStack.ContainsAsync(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ContainsMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return from x in activeStack.ContainsMany(ids, token).Zip(shadowStack.ContainsMany(ids, token))
                       select x.First | x.Second;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await using IAsyncEnumerator<bool> active = activeStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token),
                    shadow = shadowStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token);

                while (await active.MoveNextAsync() && await shadow.MoveNextAsync())
                    yield return active.Current || shadow.Current;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ContainsManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await using IAsyncEnumerator<bool> active = activeStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token),
                    shadow = shadowStack.ContainsManyAsync(ids, token).GetAsyncEnumerator(token);

                while (await active.MoveNextAsync() && await shadow.MoveNextAsync())
                    yield return active.Current || shadow.Current;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual bool IsRented(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return activeStack.Contains(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> IsRentedAsync(TID id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                return await activeStack.ContainsAsync(id, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> IsRentedMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
                return activeStack.ContainsMany(ids, token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> IsRentedManyAsync(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (bool value in activeStack.ContainsManyAsync(ids, token))
                    yield return value;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> IsRentedManyAsync(IAsyncEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
                await foreach (bool value in activeStack.ContainsManyAsync(ids, token))
                    yield return value;
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Modify

        #region Single

        /// <inheritdoc/>
        public virtual bool Modify<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual bool Modify<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsync<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsyncA<TUpdateableModel>(TID id, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsync<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyAsyncA<TUpdateableModel>(TUpdateableModel src, TUpdateableModel mod, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Many

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ModifyMany<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ModifyMany<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ModifyManyAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, [EnumeratorCancellation] CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Many ignore

        /// <inheritdoc/>
        public virtual bool ModifyManyIgnore<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual bool ModifyManyIgnore<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            semaphore.Wait(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsync<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IAsyncEnumerable<(TID, TUpdateableModel)> idWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ModifyManyIgnoreAsyncA<TUpdateableModel>(IAsyncEnumerable<(TUpdateableModel, TUpdateableModel)> srcWithMods, CancellationToken token = default)
            where TUpdateableModel : IAsyncUpdateableModel<TID>, TModel
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            try
            {
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
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #endregion

        #region Rent/Return

        /// <inheritdoc/>
        public virtual TModel Rent(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<TModel> RentAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TModel> RentMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<TModel> RentManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<TModel> RentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool Return(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ReturnAsync(TModel model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<bool> ReturnMany(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ReturnManyAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<bool> ReturnManyAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool ReturnManyIgnore(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ReturnManyIgnoreAsync(IEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ReturnManyIgnoreAsync(IAsyncEnumerable<TModel> models, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual bool TryRent(TID id, out TModel? model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async Task<(bool Success, TModel? Result)> TryRentAsync(TID id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<(bool Success, TModel? Result)> TryRentMany(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(bool Success, TModel? Result)> TryRentManyAsync(IAsyncEnumerable<TID> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Subscribe/Desubscribe

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
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> SubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TModel> model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<int> DesubscribeManyAsync(IAsyncEnumerable<TID> id, CancellationToken token = default)
        {
            throw new NotImplementedException();
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
        public virtual IEnumerator<TID> EnumerateIDs(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerator<TID> EnumerateIDsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerator<TModel> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerator<TID> GetEnumerator(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerator<TModel> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}