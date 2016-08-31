using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;

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
            => base.OnFilter(obj) && this.FilterBySelected(obj) && this.FilterByText(obj);

        private bool FilterByText(FlagViewModel obj)
        {
            Debug.Assert(this.filters != null);
            return this.filters.Length == 0 ||
                   this.filters.Any(z => obj.Source.Value.Contains(z, StringComparison.OrdinalIgnoreCase));
        }

        private bool FilterBySelected(FlagViewModel obj) => !this.SelectedItems.Contains(obj);

        protected override bool OnResetFilter(string filterText)
        {
            this.filters = string.IsNullOrWhiteSpace(filterText)
                ? Empty<string>.Array
                : filterText.Split("|", StringSplitOptions.RemoveEmptyEntries);

            return true;
        }

        public ObservableCollection<FlagViewModel> SelectedItems { get; } = new ObservableCollection<FlagViewModel>();

        public HashSet<string> SelectedStrings { get; } = new HashSet<string>();

        public async Task LoadAsync()
        {
            const int recentlyCount = 3;
            var flags = await (await this.GetManagers().FlagManager.LoadAsync(this.Type))
                .Select(z => new FlagViewModel(z))
                .OrderByDescending(z => z.Source.Updated)
                .ToArrayAsync();
            if (flags.Length > recentlyCount)
            {
                var recent = flags.Take(recentlyCount).ToArray();
                var next = await flags.Skip(recentlyCount).OrderByDescending(z => z.Source.Count).ToArrayAsync();
                Debug.Assert(recent.Length + next.Length == flags.Length);
                flags = recent.Concat(next).ToArray();
            }
            var selected = await flags
                .Where(z => this.SelectedStrings.Contains(z.Source.Value))
                .ToArrayAsync();

            this.SelectedItems.Reset(selected);
            this.Items.Collection.Reset(flags);
        }

        public void Sync() => this.SelectedStrings.Reset(this.SelectedItems.Select(z => z.Source.Value));

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

            if (await this.GetManagers().FlagManager.RemoveAsync(item.Source.Id))
            {
                this.Items.Collection.Remove(item);
            }
        }
    }
}
