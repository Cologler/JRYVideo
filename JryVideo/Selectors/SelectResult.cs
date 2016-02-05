namespace JryVideo.Selectors
{
    public sealed class SelectResult<T>
    {
        public static SelectResult<T> NonAccept => new SelectResult<T>(false);

        public static SelectResult<T> Selected(T value) => new SelectResult<T>(true, value);

        private SelectResult(bool isAccept, T value = default(T))
        {
            this.IsAccept = isAccept;
            this.Value = value;
        }

        public bool IsAccept { get; }

        public T Value { get; }
    }
}