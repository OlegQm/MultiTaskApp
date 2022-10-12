using Android.Media;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ToothScan.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ToothScan.Views
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public partial class NoteAddingPage : ContentPage
    {
        List<string> NoteTextList = new List<string>();
        List<string> NoteTextListCopy = new List<string>(); //This list contains a reverse copy of the 'NoteTextList' list
        List<double> SymbolsScrollPositions = new List<double>();

        private bool SpeechButtonClicked = false;
        private bool ItemsRemovable = true;
        private bool IsFirstChange = true;
        private bool NoteTextFirstAdding = true;

        private string note_text = null;

        private int StartSearchIndex = -1;
        private int IndexDowngrading = 0;

        AudioManager sound_set;
        CancellationTokenSource cts;

        /**
         * Page loading
         */
        public string ItemId
        {
            set
            {
                LoadNote(value);
            }
        }
        /**
        * Hiding the SearchBar and other search elements.
        */
        private void HideAllElements()
        {
            PreviousPage.IsEnabled = true;
            PreviousPage.TextOverride = "";
            Connections.searcher_text_transmission = null;
            Connections.start_search_index = -1;
            StartSearchIndex = -1;
            CancelSerch.IsVisible = false;
            Searcher.IsVisible = false;
            NextSearchElement.IsVisible = false;
            Searcher.Text = null;
            NoteLabel.IsVisible = false;
            NoteLabel.Text = null;
            NoteEditor.IsVisible = true;
            SearchButton.IsEnabled = true;
            Clear.IsEnabled = true;
            ReplaceDoubleSpaces.IsEnabled = true;
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }
        /**
        * Handling the programmatically "Back" button click - Only for Android and UWP
        */
        protected override bool OnBackButtonPressed()
        {
            if (NoteLabel.IsVisible == true)
            {
                HideAllElements();
            }
            else
            {
                Navigation.PopAsync();
            }
            base.OnBackButtonPressed();
            return true;
        }
        public NoteAddingPage()
        {
            InitializeComponent();

            BindingContext = new Note();
            Searcher.IsVisible = false;
            NextSearchElement.IsVisible = false;
            CancelSerch.IsVisible = false;
            NoteLabel.IsVisible = false;
        }

        /**
         * Download note, save current id and date
         */
        private void Searcher_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue == null || e.NewTextValue == "")
                NoteLabel.Text = note_text.Replace("\n", "<br>");
            Connections.start_search_index = 0;
            StartSearchIndex = 0;
        }
        /**
        * Disabling Back and Next buttons on appearing
        */
        protected override void OnAppearing()
        {
            base.OnAppearing();
            BackButton.IsEnabled = false;
            NextButton.IsEnabled = false;
        }

        /**
        * Reset element values ​​on disappearing
        */
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            note_text = null;
            NoteLabel.Text = null;
            Searcher.Text = null;
            Connections.searcher_text_transmission = null;
            Connections.start_search_index = -1;
            NoteTextList.Clear();
            NoteTextListCopy.Clear();
            CancelSpeech();
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }
        /**
         * Download note, save current id and date
         */
        private async void LoadNote(string value)
        {
            try
            {
                int id = Convert.ToInt32(value);
                Note note = await App.NotesDB.GetNoteAsync(id);
                BindingContext = note;
                NoteTextList.Add(note.Text);
            }
            catch
            {
                await DisplayAlert("Something was wrong",
                    "Please, try again",
                    "OK");
            }
        }
        /**
         * Save the text and date of the note, return to the list of notes
         */
        private async void OnSaveButton_Clicked(object sender, EventArgs e)
        {
            Note note = (Note)BindingContext;
            note.Date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(note.Text))
            {
                await App.NotesDB.SaveNoteAsync(note);
            }
            else
            {
                await App.NotesDB.DeleteNoteAsync(note);
                CrossToastPopUp.Current.ShowToastMessage("No text in the note");
            }
            await Shell.Current.GoToAsync("..");
        }
        /**
         * Delete the note, return to the list of notes
         */
        private async void OnDeleteButton_Clicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Remove Note",
                "Do you want to delete note?",
                "Delete",
                "Cancel");
            if (result)
            {
                Note note = (Note)BindingContext;
                await App.NotesDB.DeleteNoteAsync(note);
                await Shell.Current.GoToAsync("..");
            }
            else
                return;
        }
        /**
         * Clearing page text, hiding search items
         */
        private void Clear_Clicked(object sender, EventArgs e)
        {
            if (NoteEditor.Text != null)
                NoteEditor.Text = null;
            if (Searcher.Text != null)
                Searcher.Text = null;
            if (NoteLabel.Text != null)
                NoteLabel.Text = null;

            Connections.searcher_text_transmission = null;
            Connections.start_search_index = -1;
            Searcher.IsVisible = false;
            NextSearchElement.IsVisible = false;
            CancelSerch.IsVisible = false;
            NoteLabel.IsVisible = false;
            NoteEditor.IsVisible = true;
        }
        /**
         * Including all the elements you need to search, opening the keyboard,
           as well as formatting and saving text in a variable for HTML format
         */
        private async void SearchButton_Clicked(object sender, EventArgs e)
        {
            if (NoteEditor.Text != null)
            {
                PreviousPage.IsEnabled = false;
                PreviousPage.TextOverride = "\0";
                SearchButton.IsEnabled = false;
                ReplaceDoubleSpaces.IsEnabled = false;
                Clear.IsEnabled = false;
                note_text = NoteEditor.Text;
                NoteLabel.Text = note_text.Replace("\n", "<br>");
                Searcher.IsVisible = true;
                NextSearchElement.IsVisible = true;
                CancelSerch.IsVisible = true;
                NoteEditor.IsVisible = false;
                NoteLabel.IsVisible = true;
                Connections.start_search_index = 0;
                StartSearchIndex = 0;
                Searcher.Focus();
            }
            else
            {
                await DisplayAlert("Warning",
                    "Write some text.",
                    "OK");
            }
        }
        /**
         * Text back implementation: Using 'symbols_scroll_bar_positions' (a list that stores the previous 10 versions of the text)
           we can switch between versions of the text, thereby returning it, also in this code fragment, the implementation of auto-scrolling of the text is implemented,
           which uses the list 'symbols_scroll_bar_positions', which stores the coordinates of the symbols relative to the Editor
         */
        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            NoteEditor.IsReadOnly = true;
            if (NoteEditor.IsVisible == true && NoteTextList.Count >= 1)
            {
                if ((SymbolsScrollPositions.Count - IndexDowngrading) >= 0)
                {
                    IndexDowngrading += 1;
                }
                NoteTextListCopy.Add(NoteTextList[NoteTextList.Count - 1]);
                NoteTextList.Remove(NoteTextList[NoteTextList.Count - 1]);

                NoteTextListCopy = NoteTextListCopy.Distinct().ToList();

                ItemsRemovable = false;
                NoteEditor.Text = NoteTextListCopy[NoteTextListCopy.Count - 1];

                await Task.Delay(20);
                if ((SymbolsScrollPositions.Count - IndexDowngrading) >= 0)
                {
                    if((SymbolsScrollPositions.Count - IndexDowngrading) <= (SymbolsScrollPositions.Count - 1))
                        await Scroll.ScrollToAsync(0, SymbolsScrollPositions[SymbolsScrollPositions.Count - IndexDowngrading], false);
                }
                ItemsRemovable = true;

                NoteTextFirstAdding = false;

                NextButton.IsEnabled = true;
            }
            if (NoteTextList.Count < 1)
            {
                BackButton.IsEnabled = false;
            }
            NoteEditor.IsReadOnly = false;
        }
        /**
         * Implementation of reverting text to its original state: Using 'symbols_scroll_bar_positions_for_next_note' (a list that stores the next 10 versions of the text)
           we can switch between versions of the text, thereby returning it to its original state, also in this code fragment, the implementation of auto-scrolling of the text is implemented,
           which uses the list 'symbols_scroll_bar_positions', which stores the coordinates of the symbols relative to the Editor
         */
        private async void NextButton_Clicked(object sender, EventArgs e)
        {
            NoteEditor.IsReadOnly = true;
            if (NoteEditor.IsVisible == true && NoteTextListCopy.Count >= 1)
            {
                if ((SymbolsScrollPositions.Count - IndexDowngrading) <= (SymbolsScrollPositions.Count - 1))
                {
                    IndexDowngrading -= 1;
                }
                NoteTextList.Add(NoteTextListCopy[NoteTextListCopy.Count - 1]);
                NoteTextListCopy.Remove(NoteTextListCopy[NoteTextListCopy.Count - 1]);

                NoteTextList = NoteTextList.Distinct().ToList();

                ItemsRemovable = false;
                NoteEditor.Text = NoteTextList[NoteTextList.Count - 1];

                await Task.Delay(20);
                if ((SymbolsScrollPositions.Count - IndexDowngrading) <= (SymbolsScrollPositions.Count - 1))
                {
                    if(SymbolsScrollPositions.Count - IndexDowngrading >= 0)
                        await Scroll.ScrollToAsync(0, SymbolsScrollPositions[SymbolsScrollPositions.Count - IndexDowngrading], false);
                }
                ItemsRemovable = true;

                BackButton.IsEnabled = true;
            }
            if (NoteTextListCopy.Count < 1)
            {
                NextButton.IsEnabled = false;
            }
            NoteEditor.IsReadOnly = false;
        }
        /**
        * In XAML, we set the HTML text type to 'note_label', after clicking the search button, all the searchable elements change the background to red
          private async void Searcher_SearchButtonPressed(object sender, EventArgs e)
        */
        private async void Searcher_SearchButtonPressed(object sender, EventArgs e)
        {
            int index_of_first_word = note_text.IndexOf(Searcher.Text);

            if (index_of_first_word != -1)
            {
                if (Searcher.Text != null && Searcher.Text != "" && note_text != null)
                {
                    string notes_text = note_text;
                    string searcher_txt = Searcher.Text;
                    string html = "<span style=\"background-color:red\">" + searcher_txt + "</span>";
                    notes_text = notes_text.Replace(searcher_txt, html);
                    notes_text = notes_text.Replace("\n", "<br/>");
                    NoteLabel.Text = notes_text;
                    CrossToastPopUp.Current.ShowToastMessage("Ready!");
                }
                else
                {
                    await DisplayAlert("Warning",
                        "Write some text.",
                        "Ok");
                }
            }
            else
            {
                CrossToastPopUp.Current.ShowToastMessage("Nothing was found");
            }
        }
        /**
        * The "Next Element" system is implemented as follows: there is an Editor (with the ability to change the text) and a Label (with the same text, but without
          the ability to change the text), while searching we have the Label open so that the text isn't changed by the user, but when the user clicks the 'Next_search_element' button,
          the Label disappears, an Editor appears instead, in which the cursor position to the next element is set, then the Editor disappears, and the Label appears, thus scrolling is carried out
        */
        private async void NextSearchElement_Clicked(object sender, EventArgs e)
        {
            Connections.searcher_text_transmission = Searcher.Text;
            if (Searcher.Text != null && Searcher.Text != "" && note_text != null)
            {
                NoteEditor.IsVisible = false;
                NoteLabel.IsVisible = true;
                int index_of_first_word = note_text.IndexOf(Searcher.Text, Connections.start_search_index);
                if (index_of_first_word != -1)
                {
                    string notes_text = note_text;
                    notes_text = notes_text.Replace("\n", "<br>");
                    string searcher_txt = Searcher.Text;
                    searcher_txt = searcher_txt.Replace("\n", "<br>");
                    string html = "<span style=\"background-color:red\">" + searcher_txt + "</span>";
                    string html_current = "<span style=\"background-color:blue\">" + searcher_txt + "</span>";
                    notes_text = notes_text.Replace(searcher_txt, html);
                    StartSearchIndex = notes_text.IndexOf(html, StartSearchIndex);
                    if (StartSearchIndex != -1)
                    {
                        notes_text = notes_text.Remove(StartSearchIndex, html.Length);
                        notes_text = notes_text.Insert(StartSearchIndex, html_current);
                        StartSearchIndex += Searcher.Text.Length;
                        NoteLabel.Text = notes_text;

                        NoteLabel.IsVisible = false;
                        NoteEditor.IsVisible = true;
                        NoteEditor.IsVisible = false;
                        NoteLabel.IsVisible = true;
                    }
                    else
                    {

                        CrossToastPopUp.Current.ShowToastMessage("Nothing was found");
                        Connections.start_search_index = 0;
                        StartSearchIndex = 0;
                    }

                    Connections.start_search_index = index_of_first_word + Searcher.Text.Length;
                }
                else
                {
                    CrossToastPopUp.Current.ShowToastMessage("Nothing was found");
                    Connections.start_search_index = 0;
                    StartSearchIndex = 0;
                }
            }
            else
            {
                await DisplayAlert("Warning",
                    "Write some text.",
                    "Ok");
            }
        }
        /**
        * Forced speech stop (in case the text was voiced)
        */
        private void CancelSearch_Clicked(object sender, EventArgs e)
        {
            HideAllElements();
        }
        /**
         * Copy text to clipboard
         */
        private async void CopyButton_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(NoteEditor.Text);
            CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard");
        }
        /**
         * While changing the text, we form a list of the previous text (no more than 10 characters at a time), as well as a list of coordinates of all
           added/removed elements on the page; the value of some variables is also changed for the correct operation of Back_bote and Next_note
         */
        private async void NoteEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            Note note = (Note)BindingContext;
            note.Date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(note.Text))
                await App.NotesDB.SaveNoteAsync(note);

            if (ItemsRemovable && !IsFirstChange)
            {
                IndexDowngrading = 0;
                NoteTextFirstAdding = true;
                BackButton.IsEnabled = true;
                NoteTextList.Add(e.NewTextValue);
                SymbolsScrollPositions.Add(Scroll.ScrollY);
                if (NoteTextList.Count > 11)
                {
                    NoteTextList.RemoveAt(0);
                    SymbolsScrollPositions.RemoveAt(0);
                }
            }
            IsFirstChange = false;
        }
        /**
         * Scroll down the page
         */
        private async void ScrollToEnd_Clicked(object sender, EventArgs e)
        {
            await Scroll.ScrollToAsync(DeleteButton, ScrollToPosition.End, true);
        }
        /**
         * Scroll up the page
         */
        private async void ScrollToBegin_Clicked(object sender, EventArgs e)
        {
            if (NoteEditor.IsVisible == true)
            {
                await Scroll.ScrollToAsync(NoteEditor, ScrollToPosition.Start, true);
            }
            else if (NoteLabel.IsVisible == true)
            {
                await Scroll.ScrollToAsync(NoteLabel, ScrollToPosition.Start, true);
            }
            else
            {
                await DisplayAlert("Warning",
                    "Something was wrong",
                    "OK");
            }
        }
        /**
         * Setting preferences for text playback
         */
        public async Task SpeakNowDefaultSettings()
        {
            cts = new CancellationTokenSource();
            try
            {
                await TextToSpeech.SpeakAsync(NoteEditor.Text, cancelToken: cts.Token);
            }
            catch
            {
                await DisplayAlert(null,
                    "Something was wrong",
                    "OK");
            }
        }
        /**
         * Stop speech
         */
        public void CancelSpeech()
        {
            if (cts?.IsCancellationRequested ?? true)
                return;

            cts.Cancel();
        }
        /**
         * Calling the functions of playing or canceling speech, depending on the number of clicks on the button
         */
        private async void VoiceText_Clicked(object sender, EventArgs e)
        {
            sound_set = Connections.sound_lvl;
            if (sound_set.GetStreamVolume(Stream.Music) != 0)
            {
                if (SpeechButtonClicked == false)
                {
                    SpeechButtonClicked = true;
                    VoiceText.Text = "Cancel speech";
                    if (NoteEditor.IsVisible == true)
                    {
                        await SpeakNowDefaultSettings();
                        VoiceText.Text = "Voice text";
                    }
                    else if (NoteLabel.IsVisible == true)
                    {
                        await SpeakNowDefaultSettings();
                        VoiceText.Text = "Voice text";
                    }
                    else
                    {
                        await DisplayAlert("Warning",
                            "Something was wrong.",
                            "OK");
                    }
                }
                else
                {
                    CancelSpeech();
                    VoiceText.Text = "Voice text";
                    SpeechButtonClicked = false;
                }
            }
            else
            {
                CrossToastPopUp.Current.ShowToastMessage("Sound is OFF");
            }
        }
        /**
         * Replacing double spaces with single spaces
         */
        private void ReplaceDoubleSpaces_Clicked(object sender, EventArgs e)
        {
            NoteEditor.Text = NoteEditor.Text.Replace("  ", " ");
        }
        /**
         * Creating a list of options for changing text color in the Editor
         */
        private async void TextOptions_Clicked(object sender, EventArgs e)
        {
            var actionSheet = await DisplayActionSheet("Choose note text color", "Cancel", null, "Red", "Green", "Blue",
                "Aqua", "Fuchsia", "Yellow", "Gray", "Black", "White");
            if (actionSheet == "Cancel" || actionSheet == null)
            {
                return;
            }
            else
            {
                NoteEditor.TextColor = (Color)typeof(Color).GetRuntimeField(actionSheet).GetValue(null);
                NoteLabel.TextColor = (Color)typeof(Color).GetRuntimeField(actionSheet).GetValue(null);
            }
        }
        /**
        * Creating a list of options for changing background color in the Editor
        */
        private async void BackgroundOptions_Clicked(object sender, EventArgs e)
        {
            var actionSheet = await DisplayActionSheet("Choose note text color", "Cancel", null, "Red", "Green", "Blue",
                "Aqua", "Fuchsia", "Yellow", "Gray", "Black", "White");
            if (actionSheet == "Cancel" || actionSheet == null)
            {
                return;
            }
            else
            {
                BackgroundColor = (Color)typeof(Color).GetRuntimeField(actionSheet).GetValue(null);
            }
        }
    }
}