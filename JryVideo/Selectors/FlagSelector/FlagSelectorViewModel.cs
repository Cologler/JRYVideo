using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Selectors.Common;

namespace JryVideo.Selectors.FlagSelector
{
    public sealed class FlagSelectorViewModel : BaseSelectorViewModel<FlagViewModel>
    {
        public JryFlagType Type { get; private set; }

        public FlagSelectorViewModel(JryFlagType type)
        {
            this.Type = type;
            this.SelectedItems = new ObservableCollection<FlagViewModel>();
        }

        protected override bool OnFilter(FlagViewModel obj)
        {
            return this.FilterBySelected(obj) && this.FilterByText(obj);
        }

        private bool FilterByText(FlagViewModel obj)
        {
            if (String.IsNullOrWhiteSpace(this.FilterText))
            {
                return true;
            }
            else
            {
                return obj.Source.Value.Contains(this.FilterText.Trim());
            }
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
            this.Items.Collection.AddRange(items.Select(z => new FlagViewModel(z)));
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
    }
}
