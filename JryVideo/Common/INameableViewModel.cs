namespace JryVideo.Common
{
    public interface INameableViewModel
    {
        string FirstLine { get; }

        string FullName { get; }

        string FullNameLine { get; }

        string SecondLine { get; }
    }
}