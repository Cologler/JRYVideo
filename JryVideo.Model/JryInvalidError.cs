namespace JryVideo.Model
{
    public enum JryInvalidError
    {
        // all
        ObjectInitializeFailed,
        ObjectMetaDataCreateFailed,

        // series
        SeriesNamesCanNotBeEmpty,

        // video
        VideoTypeCanNotBeEmpty,
        VideoYearValueInvalid,
        VideoIndexLessThanOne,
        VideoEpisodesCountLessThanOne,

        // cover
        CoverBinaryCanNotBeEmpty,

        // counter
        CounterCountLessThanOne,

        // artist & counter
        NameCanNotBeEmpty,
        
    }
}