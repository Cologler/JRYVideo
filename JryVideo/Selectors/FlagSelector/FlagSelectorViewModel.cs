using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Selectors.FlagSelector
{
    public sealed class FlagSelectorViewModel : BaseSelectorViewModel<FlagViewModel, JryFlag>
    {
        private readonly IEnumerable<string> readySelected;

        public JryFlagType Type { get; }

        public FlagSelectorViewModel(JryFlagType type, IEnumerable<string> readySelected)
        {
            this.readySelected = readySelected;
            this.Type = type;
            this.SelectedItems = new ObservableCollection<FlagViewModel>();
        }

        protected override bool OnFilter(FlagViewModel obj)
        {
            return base.OnFilter(obj) && this.FilterBySelected(obj) && this.FilterByText(obj);
        }

        private bool FilterByText(FlagViewModel obj)
        {
            return string.IsNullOrWhiteSpace(this.FilterText) ||
                obj.Source.Value.ToLower().Contains(this.FilterText.Trim().ToLower());
        }

        private bool FilterBySelected(FlagViewModel obj)
        {
            return !this.SelectedItems.Contains(obj);
        }

        public ObservableCollection<FlagViewModel> SelectedItems { get; private set; }

        public async Task LoadAsync()
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.FlagManager;

            var items = (await manager.LoadAsync(this.Type)).ToArray();

            this.Items.Collection.Clear();
            foreach (var item in items.Select(z => new FlagViewModel(z)))
            {
                if (this.readySelected.Contains(item.Source.Value))
                    this.SelectedItems.Add(item);

                this.Items.Collection.Add(item);
            }
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
