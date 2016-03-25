using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Selectors.FlagSelector
{
    public sealed class FlagSelectorViewModel : BaseSelectorViewModel<FlagViewModel, JryFlag>
    {
        private string[] filters = Empty<string>.Array;

        public JryFlagType Type { get; }

        public FlagSelectorViewModel(JryFlagType type)
        {
            this.Type = type;
        }

        protected override bool OnFilter(FlagViewModel obj)
        {
            return base.OnFilter(obj) && this.FilterBySelected(obj) && this.FilterByText(obj);
        }

        private bool FilterByText(FlagViewModel obj)
        {
            Debug.Assert(this.filters != null);
            return this.filters.Length == 0 ||
                   this.filters.Any(z => obj.Source.Value.Contains(z, StringComparison.OrdinalIgnoreCase));
        }

        private bool FilterBySelected(FlagViewModel obj)
        {
            return !this.SelectedItems.Contains(obj);
        }

        protected override void OnResetFilter(string filterText)
        {
            base.OnResetFilter(filterText);

            this.filters = string.IsNullOrWhiteSpace(filterText)
                ? Empty<string>.Array
                : filterText.Split("|", StringSplitOptions.RemoveEmptyEntries);
        }

        public ObservableCollection<FlagViewModel> SelectedItems { get; } = new ObservableCollection<FlagViewModel>();

        public List<string> SelectedStrings { get; } = new List<string>();

        public async Task LoadAsync()
        {
            var items = (await this.GetManagers().FlagManager.LoadAsync(this.Type)).ToArray();

            this.Items.Collection.Clear();
            foreach (var item in items.Select(z => new FlagViewModel(z)))
            {
                if (this.SelectedStrings.Contains(item.Source.Value))
                    this.SelectedItems.Add(item);

                this.Items.Collection.Add(item);
            }
        }

        public void Sync()
        {
            this.SelectedStrings.Reset(this.SelectedItems.Select(z => z.Source.Value));
        }

        public void SelectItem(FlagViewModel item)
        {
            if (item == null) return;

            this.SelectedItems.Add(item);
            this.Items.View.Refresh();
        }

        public void UnselectItem(FlagViewModel item)
        {
            if (this.SelectedItems.Remove(item))
            {
                this.Items.View.Refresh();
            }
        }

        public void EditFlagUserControl_ViewModel_Created(object sender, JryFlag e)
        {
            if (this.GetUIDispatcher().CheckAccessOrBeginInvoke(
                this.EditFlagUserControl_ViewModel_Created, sender, e))
            {
                new FlagViewModel(e).BeAdd(this.SelectedItems).BeAdd(this.Items.Collection);
            }
        }

        public async Task DeleteItemAsync(FlagViewModel item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var manager = JryVideoCore.Current.CurrentDataCenter.FlagManager;
            if (await manager.RemoveAsync(item.Source.Id))
            {
                this.Items.Collection.Remove(item);
            }
        }
    }
}
