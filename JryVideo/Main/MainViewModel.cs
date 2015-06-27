using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace JryVideo.Main
{
    public class MainViewModel : JasilyViewModel
    {
        public MainViewModel()
        {
            this.VideosViewModel = new MainSeriesItemViewerViewModel();
        }

        public MainSeriesItemViewerViewModel VideosViewModel { get; private set; }
    }
}