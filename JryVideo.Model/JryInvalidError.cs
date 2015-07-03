namespace JryVideo.Model
{
    public enum JryInvalidError
    {
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
        
        // entity
    }
}