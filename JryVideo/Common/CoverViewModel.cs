using System;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class CoverViewModel<T> : HasCoverViewModel<T>
        where T : IJryCoverParent
    {
        private readonly Func<T, Task<bool>> autoCreateCover;

        protected CoverViewModel(T source, Func<T, Task<bool>> autoCreateCover)
            : base(source)
        {
            this.autoCreateCover = autoCreateCover;
        }

        protected override Task<bool> TryAutoAddCoverAsync()
            => this.autoCreateCover?.Invoke(this.Source) ?? base.TryAutoAddCoverAsync();
    }
}