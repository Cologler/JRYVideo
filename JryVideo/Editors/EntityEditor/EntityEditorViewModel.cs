using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Editable;
using System.Diagnostics;
using System.Enums;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Editors.EntityEditor
{
    public class EntityEditorViewModel : EditorItemViewModel<JryEntity>
    {
        private string resolution;
        private string filmSource;
        private string audioSource;
        private string extension;
        private bool isWildcardChecked;
        private bool isRegexChecked;
        private string format;

        public EntityEditorViewModel(Model.JryVideo video)
        {
            this.Video = video;
            this.Tags = new ObservableCollection<string>();
            this.Fansubs = new ObservableCollection<string>();
            this.SubTitleLanguages = new ObservableCollection<string>();
            this.TrackLanguages = new ObservableCollection<string>();
            this.isWildcardChecked = true;

            this.Resolutions = new List<string>();
            this.FilmSources = new List<string>();
            this.AudioSources = new List<string>();
            this.Extensions = new List<string>();
        }

        public Model.JryVideo Video { get; private set; }
        
        public ObservableCollection<string> this[JryFlagType flagType]
        {
            get
            {
                switch (flagType)
                {
                    case JryFlagType.EntityFansub:
                        return this.Fansubs;

                    case JryFlagType.EntitySubTitleLanguage:
                        return this.SubTitleLanguages;

                    case JryFlagType.EntityTrackLanguage:
                        return this.TrackLanguages;

                    case JryFlagType.EntityTag:
                        return this.Tags;

                    default:
                        throw new ArgumentOutOfRangeException("flagType", flagType, null);
                }
            }
        }

        public ObservableCollection<string> Tags { get; private set; }

        public ObservableCollection<string> Fansubs { get; private set; }

        public ObservableCollection<string> SubTitleLanguages { get; private set; }

        public ObservableCollection<string> TrackLanguages { get; private set; }

        public List<string> Resolutions { get; private set; }

        public List<string> FilmSources { get; private set; }

        public List<string> AudioSources { get; private set; }

        public List<string> Extensions { get; private set; }

        public async Task LoadAsync()
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.FlagManager;

            this.Resolutions.AddRange((await manager.LoadAsync(JryFlagType.EntityResolution)).Select(z => z.Value));
            this.FilmSources.AddRange((await manager.LoadAsync(JryFlagType.EntityFilmSource)).Select(z => z.Value));
            this.AudioSources.AddRange((await manager.LoadAsync(JryFlagType.EntityAudioSource)).Select(z => z.Value));
            this.Extensions.AddRange((await manager.LoadAsync(JryFlagType.EntityExtension)).Select(z => z.Value));
        }

        [EditableField]
        public string Resolution
        {
            get { return this.resolution; }
            set { this.SetPropertyRef(ref this.resolution, value); }
        }

        [EditableField]
        public string FilmSource
        {
            get { return this.filmSource; }
            set { this.SetPropertyRef(ref this.filmSource, value); }
        }

        [EditableField]
        public string AudioSource
        {
            get { return this.audioSource; }
            set { this.SetPropertyRef(ref this.audioSource, value); }
        }

        [EditableField]
        public string Extension
        {
            get { return this.extension; }
            set { this.SetPropertyRef(ref this.extension, value); }
        }

        public bool IsWildcardChecked
        {
            get { return this.isWildcardChecked; }
            set { this.SetPropertyRef(ref this.isWildcardChecked, value); }
        }

        public bool IsRegexChecked
        {
            get { return this.isRegexChecked; }
            set { this.SetPropertyRef(ref this.isRegexChecked, value); }
        }

        public string Format
        {
            get { return this.format; }
            set { this.SetPropertyRef(ref this.format, value); }
        }

        public override void ReadFromObject(JryEntity obj)
        {
            base.ReadFromObject(obj);

            this.Tags.Clear();
            this.Tags.AddRange(obj.Tags);

            this.Fansubs.Clear();
            this.Fansubs.AddRange(obj.Fansubs);

            this.SubTitleLanguages.Clear();
            this.SubTitleLanguages.AddRange(obj.SubTitleLanguages);

            this.TrackLanguages.Clear();
            this.TrackLanguages.AddRange(obj.TrackLanguages);

            if (obj.Format != null)
            {
                switch (obj.Format.Type)
                {
                    case JryFormatType.Regex:
                        this.IsRegexChecked = true;
                        break;

                    case JryFormatType.Wildcard:
                        this.IsWildcardChecked = true;
                        break;
                }

                Debug.Assert(this.IsRegexChecked != this.IsWildcardChecked);

                this.Format = obj.Format.Value;
            }
        }

        public override void WriteToObject(JryEntity obj)
        {
            base.WriteToObject(obj);

            obj.Tags = this.Tags.Distinct().OrderBy(z => z).ToList();
            obj.Fansubs = this.Fansubs.Distinct().OrderBy(z => z).ToList();
            obj.SubTitleLanguages = this.SubTitleLanguages.Distinct().OrderBy(z => z).ToList();
            obj.TrackLanguages = this.TrackLanguages.Distinct().OrderBy(z => z).ToList();

            if (!this.Format.IsNullOrWhiteSpace())
            {
                Debug.Assert(this.IsRegexChecked != this.IsWildcardChecked);

                obj.Format = new JryFormat()
                {
                    Type = this.IsRegexChecked ? JryFormatType.Regex : JryFormatType.Wildcard,
                    Value = this.Format
                };
            }
        }

        public async Task<JryEntity> CommitAsync(MetroWindow window)
        {
            if (JryEntity.IsExtensionInvalid(this.Extension))
            {
                await window.ShowMessageAsync("error", "invalid extension.");
                return null;
            }

            if (JryEntity.IsResolutionInvalid(this.Resolution))
            {
                await window.ShowMessageAsync("error", "invalid resolution.");
                return null;
            }

            var entity = this.GetCommitObject();

            this.WriteToObject(entity);

            var provider = JryVideoCore.Current.CurrentDataCenter.VideoManager.GetEntityManager(this.Video);

            if (this.Action == ObjectChangedAction.Create)
            {
                entity.BuildMetaData();

                if (provider.IsExists(entity))
                {
                    await window.ShowMessageAsync("error", "had same entity.");
                    return null;
                }
            }

            return await base.CommitAsync(provider, entity);
        }
    }
}