// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System.Collections.ObjectModel;

namespace MOGWAI_RUNTIME.Pages;

public partial class BasicSelectorPage : ContentPage
{
    private int _initialIndex = -1;

    public string? SelectedItem
    {
        get
        {
            return ItemsCollectionView.SelectedItem as string;
        }
    }

    public int SelectedIndex
    {
        get
        {
            if (ItemsCollectionView.SelectedItem == null) return -1;

            var s = ItemsCollectionView.SelectedItem as string;

            if (s != null)
            {
                var index = Items.IndexOf(s);
                return index;
            }
            else
            {
                return -1;
            }
        }
    }

    public bool Done { get; private set; }

    public ObservableCollection<string> Items { get; } = new();

    public BasicSelectorPage(string title, List<string> items, int selectedIndex = -1)
    {
        InitializeComponent();

        TitleLabel.Text = title;

        foreach (var item in items) Items.Add(item);

        _initialIndex = selectedIndex;

        BindingContext = this;
    }

    protected override bool OnBackButtonPressed()
    {
        Navigation.PopModalAsync();
        Done = true;
        return true;
    }

    private void ValidatePathTapGesture_Tapped(object sender, TappedEventArgs e)
    {
        if (ItemsCollectionView.SelectedItem != null)
        {
            Done = true;
            Navigation.PopModalAsync();
        }
    }

    private void CancelPathTapGesture_Tapped(object sender, TappedEventArgs e)
    {
        ItemsCollectionView.SelectedItem = null;
        Done = true;
        Navigation.PopModalAsync();
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (_initialIndex > -1 && Items.Count > _initialIndex)
        {
            ItemsCollectionView.SelectedItem = Items[_initialIndex];
            ItemsCollectionView.ScrollTo(ItemsCollectionView.SelectedItem, null, ScrollToPosition.Center, false);
        }
    }

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {

    }
}