using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToothScan.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ToothScan.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotesPage : ContentPage
    {
        public NotesPage()
        {
            InitializeComponent();
        }
        /*
         * Implementing the Share Text Function
         */
        public async Task ShareText(string text)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = text,
                Title = "Share"
            });
        }
        /*
         * Loading list of notes from database into 'CollectionView'
         */
        protected override async void OnAppearing()
        {
            collectionView.ItemsSource = await App.NotesDB.GetNotesAsync();
            base.OnAppearing();
        }
        /*
         * Clear search bar text when page disappears
         */
        protected override void OnDisappearing()
        {
            NotesSearchBar.Text = null;
            base.OnDisappearing();
        }
        /*
         * Go to the note creation page when pressing the 'ADD' button
         */
        private async void AddButton_Clicked(object sender, EventArgs e)
        {
            AddButton.IsEnabled = false;
            await Shell.Current.GoToAsync(nameof(NoteAddingPage));
            NotesSearchBar.Text = null;
            AddButton.IsEnabled = true;
        }
        /*
         * Go to the note page when selecting a note from the list
         */
        private async void collectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null)
            {
                Note note = (Note)e.CurrentSelection.FirstOrDefault();
                await Shell.Current.GoToAsync(
                    $"{nameof(NoteAddingPage)}?{nameof(NoteAddingPage.ItemId)}={note.ID.ToString()}");
                NotesSearchBar.Text = null;
            }
        }
        /*
         * Deletes a note when the user presses the delete button (swipe left),
           and also updates the list of notes after deletion
         */
        private async void OnDelete(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Remove Note", "Do you want to delete note?", "Delete", "Cancel");
            if (result)
            {
                SwipeItem swipeview = sender as SwipeItem;
                Note note = (Note)swipeview.CommandParameter;
                await App.NotesDB.DeleteNoteAsync(note);
                collectionView.ItemsSource = await App.NotesDB.GetNotesAsync();
            }
            else
            {
                return;
            }
        }
        /*
           The implementation of the function to share the text
           when the user selects the share button (swipe to the left),
           allows you to send the text of the note to a specific application
         */
        private async void OnShare(object sender, EventArgs e)
        {
            SwipeItem swipeview = sender as SwipeItem;
            Note note = (Note)swipeview.CommandParameter;
            await ShareText(note.Text);
        }
        /*
           Implementation of the note search function: this
           function checks if the given substring (in lower case) is
           contained in the given note (also in lower case),
           and then shows all notes containing the given substring
         */
        private async void Handle_SearchButtonPressed(object sender, EventArgs e)
        {
            List<Note> all_notes = await App.NotesDB.GetNotesAsync();
            List<Note> findNotes = new List<Note>();
            for (int i = 0; i < all_notes.Count; i++)
            {
                if (Convert.ToString(all_notes[i].Text).ToLower().Contains(NotesSearchBar.Text.ToLower()))
                    findNotes.Add(all_notes[i]);
            }
            if (findNotes.Count != 0)
                collectionView.ItemsSource = findNotes;
            else
                CrossToastPopUp.Current.ShowToastMessage("Nothing was found");
            all_notes.Clear();
            findNotes.Clear();
        }
        /*
           A function that checks if the search string is empty
           and displays all available notes on the screen if it is.
         */
        private async void NotesSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(e.NewTextValue == null || e.NewTextValue =="")
                collectionView.ItemsSource = await App.NotesDB.GetNotesAsync();
        }
    }
}