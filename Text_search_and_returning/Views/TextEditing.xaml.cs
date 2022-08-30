using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Text_search_and_returning
{
    public partial class TextEditing : ContentPage
    {
        List<string> temp_note_text = new List<string>();
        List<string> temp_note_text_reverce_copy = new List<string>();
        List<double> symbols_scroll_bar_positions = new List<double>();

        private bool temp_note_text_items_removable = true;
        private bool is_first_page_change = true;
        private bool is_first_adding_to_back_note = true;

        private string note_text = null;

        private int start_search_label_index = -1;
        private int current_scroll_bar_minus_index = 0;

        //скрытие SearchBar и других элементов поиска (hiding the SearchBar and other search elements)
        public void BackAllElements()
        {
            Connections.searcher_text_transmission = null;
            Connections.start_search_index = -1;
            start_search_label_index = -1;
            Searcher.IsVisible = false;
            Cancel_search.IsVisible = false;
            Next_search_element_btn.IsVisible = false;
            Searcher.Text = null;
            note_label.IsVisible = false;
            note_label.Text = null;
            note_editor.IsVisible = true;
            Search_btn.IsEnabled = true;
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }
        //Обработка нажатия кнопки "Назад" (Handling the "Back" button click) - Only for Android and UWP
        protected override bool OnBackButtonPressed()
        {
            if (note_label.IsVisible == true)
            {
                BackAllElements();
            }
            base.OnBackButtonPressed();
            return true;
        }
        public TextEditing()
        {
            InitializeComponent();
            Searcher.IsVisible = false;
            Next_search_element_btn.IsVisible = false;
            Cancel_search.IsVisible = false;
            note_label.IsVisible = false;
        }

        private void Searcher_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue == null || e.NewTextValue == "")
                note_label.Text = note_text.Replace("\n", "<br>");
            Connections.start_search_index = 0;
            start_search_label_index = 0;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Back_note.IsEnabled = false;
            Next_note.IsEnabled = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            note_text = null;
            note_label.Text = null;
            Searcher.Text = null;
            Connections.searcher_text_transmission = null;
            Connections.start_search_index = -1;
            temp_note_text.Clear();
            temp_note_text_reverce_copy.Clear();
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }

        private void Clear_note_Clicked(object sender, EventArgs e)
        {
            if (note_editor.Text != null)
                note_editor.Text = null;
            if (Searcher.Text != null)
                Searcher.Text = null;
            if (note_label.Text != null)
                note_label.Text = null;

            Connections.searcher_text_transmission = null;
            Connections.start_search_index = -1;
            Searcher.IsVisible = false;
            Next_search_element_btn.IsVisible = false;
            Cancel_search.IsVisible = false;
            note_label.IsVisible = false;
            note_editor.IsVisible = true;
        }

        private async void Search_btn_Clicked(object sender, EventArgs e)
        {
            if (note_editor.Text != null)
            {
                Search_btn.IsEnabled = false;
                note_text = note_editor.Text;
                note_label.Text = note_text.Replace("\n", "<br>");
                Searcher.IsVisible = true;
                Next_search_element_btn.IsVisible = true;
                Cancel_search.IsVisible = true;
                note_editor.IsVisible = false;
                note_label.IsVisible = true;
                Connections.start_search_index = 0;
                start_search_label_index = 0;
                Searcher.Focus();
            }
            else
            {
                await DisplayAlert("Warning", "Write some text.", "OK");
            }
        }

        private void Cancel_search_Clicked(object sender, EventArgs e)
        {
            BackAllElements();
        }

        // (RUS) В XAML мы задали HTML тип текста у 'note_label', после нажатия кнопки поиска у всех нужных (искомых) элементов фон заменяется на красный
        // (ENG) In XAML, we set the HTML text type to 'note_label', after clicking the search button, all the searchable elements change the background to red
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
                    note_label.Text = notes_text;
                    try
                    {
                        CrossToastPopUp.Current.ShowToastMessage("Ready!");
                    }
                    catch
                    {
                        await DisplayAlert("Pop up error", "Try to downgrade 'minSdkVersion' to 16 and 'targetSdkVersion' to 28 OR use 'DisplayAlert'", "Got it!");
                    }
                }
                else
                {
                    await DisplayAlert("Warning", "Write some text.", "Ok");
                }
            }
            else
            {
                try
                {
                    CrossToastPopUp.Current.ShowToastMessage("Nothing was found");
                }
                catch
                {
                    await DisplayAlert("Pop up error", "Try to downgrade 'minSdkVersion' to 16 and 'targetSdkVersion' to 28 OR use 'DisplayAlert'", "Got it!");
                }
            }
        }

        // (RUS) Во время изменения текста мы формируем список предыдущего текста (не больше 10 символов в одно время), а также список координат всех
        // добавленных/удалённых элементов на странице; также изменяется значение некоторых переменных для корректной работы Back_bote и Next_note
        // (ENG) While changing the text, we form a list of the previous text (no more than 10 characters at a time), as well as a list of coordinates of all
        // added/removed elements on the page; the value of some variables is also changed for the correct operation of Back_bote and Next_note
        private void note_editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (temp_note_text_items_removable && !is_first_page_change)
            {
                current_scroll_bar_minus_index = 0;
                is_first_adding_to_back_note = true;
                Back_note.IsEnabled = true;
                temp_note_text.Add(e.NewTextValue);
                symbols_scroll_bar_positions.Add(scroll.ScrollY);

                if (temp_note_text.Count > 11)// Ограничение в длине списков (чтобы не засорять память) (Limit on the length of lists (so as not to clutter up memory))
                {
                    temp_note_text.RemoveAt(0);
                    symbols_scroll_bar_positions.RemoveAt(0);
                }
            }
            is_first_page_change = false;
        }

        // (RUS) Реализация возврта текста: Пользуясь 'symbols_scroll_bar_positions' (список, в котором хранится предыдущие 10 версий текста)
        // мы можем переходить между версиями текста, тем самым возвращая его, также в этом фрагменте кода осуществлена реализация автопролистывания текста,
        // в которой используется список 'symbols_scroll_bar_position', в котором хранятся координаты символов относительно Editor-а
        // (ENG) Text back implementation: Using 'symbols_scroll_bar_positions' (a list that stores the previous 10 versions of the text)
        // we can switch between versions of the text, thereby returning it, also in this code fragment, the implementation of auto-scrolling of the text is implemented,
        // which uses the list 'symbols_scroll_bar_positions', which stores the coordinates of the symbols relative to the Editor
        private async void Back_note_Clicked(object sender, EventArgs e)
        {
            note_editor.IsReadOnly = true;
            if (note_editor.IsVisible == true && temp_note_text.Count >= 1)
            {
                if ((symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index) >= 0)
                {
                    current_scroll_bar_minus_index += 1;
                }
                temp_note_text_reverce_copy.Add(temp_note_text[temp_note_text.Count - 1]);
                temp_note_text.Remove(temp_note_text[temp_note_text.Count - 1]);

                temp_note_text_reverce_copy = temp_note_text_reverce_copy.Distinct().ToList();

                temp_note_text_items_removable = false;
                note_editor.Text = temp_note_text_reverce_copy[temp_note_text_reverce_copy.Count - 1];

                await Task.Delay(20);
                if ((symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index) >= 0)
                {
                    if ((symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index) <= (symbols_scroll_bar_positions.Count - 1))
                        await scroll.ScrollToAsync(0, symbols_scroll_bar_positions[symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index], false);
                }
                temp_note_text_items_removable = true;

                is_first_adding_to_back_note = false;

                Next_note.IsEnabled = true;
            }
            if (temp_note_text.Count < 1)
            {
                Back_note.IsEnabled = false;
            }
            note_editor.IsReadOnly = false;
        }

        // (RUS) Реализация возврта текста к исходному состоянию: Пользуясь 'symbols_scroll_bar_positions_for_next_note' (список, в котором хранится следующие 10 версий текста)
        // мы можем переходить между версиями текста, тем самым возвращая его к исходному состоянию, также в этом фрагменте кода осуществлена реализация автопролистывания текста,
        // в которой используется список 'symbols_scroll_bar_positions', в котором хранятся координаты символов относительно Editor-а
        // (ENG) Implementation of reverting text to its original state: Using 'symbols_scroll_bar_positions_for_next_note' (a list that stores the next 10 versions of the text)
        // we can switch between versions of the text, thereby returning it to its original state, also in this code fragment, the implementation of auto-scrolling of the text is implemented,
        // which uses the list 'symbols_scroll_bar_positions', which stores the coordinates of the symbols relative to the Editor
        private async void Next_note_Clicked(object sender, EventArgs e)
        {
            note_editor.IsReadOnly = true;
            if (note_editor.IsVisible == true && temp_note_text_reverce_copy.Count >= 1)
            {
                if ((symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index) <= (symbols_scroll_bar_positions.Count - 1))
                {
                    current_scroll_bar_minus_index -= 1;
                }
                temp_note_text.Add(temp_note_text_reverce_copy[temp_note_text_reverce_copy.Count - 1]);
                temp_note_text_reverce_copy.Remove(temp_note_text_reverce_copy[temp_note_text_reverce_copy.Count - 1]);

                temp_note_text = temp_note_text.Distinct().ToList();

                temp_note_text_items_removable = false;
                note_editor.Text = temp_note_text[temp_note_text.Count - 1];

                await Task.Delay(20);
                if ((symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index) <= (symbols_scroll_bar_positions.Count - 1))
                {
                    if (symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index >= 0)
                        await scroll.ScrollToAsync(0, symbols_scroll_bar_positions[symbols_scroll_bar_positions.Count - current_scroll_bar_minus_index], false);
                }
                temp_note_text_items_removable = true;

                Back_note.IsEnabled = true;
            }
            if (temp_note_text_reverce_copy.Count < 1)
            {
                Next_note.IsEnabled = false;
            }
            note_editor.IsReadOnly = false;
        }

        // (RUS) Система "Следующего элемента" осущетсвлена следующим образом: есть Editor (с возможностью изменения текста) и Label (с таким же текстом, но без
        // возможности изменения текста), во время поиска у нас открыт Label, чтобы текст не менялся пользователем, но когда пользователь нажимает кноку 'Next_search_element',
        // то Label исчезает, вместо него появляется Editor, в котром устанавливается позиция курсора к следующему элементу, затем Editor исчезает, а Label появляется, таким образом осуществлено пролистывание
        // (ENG) The "Next Element" system is implemented as follows: there is an Editor (with the ability to change the text) and a Label (with the same text, but without
        // the ability to change the text), while searching we have the Label open so that the text isn't changed by the user, but when the user clicks the 'Next_search_element' button,
        // the Label disappears, an Editor appears instead, in which the cursor position to the next element is set, then the Editor disappears, and the Label appears, thus scrolling is carried out

        private async void Next_search_element_btn_Clicked(object sender, EventArgs e)
        {
            Connections.searcher_text_transmission = Searcher.Text;
            if (Searcher.Text != null && Searcher.Text != "" && note_text != null)
            {
                note_editor.IsVisible = false;
                note_label.IsVisible = true;
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
                    start_search_label_index = notes_text.IndexOf(html, start_search_label_index);
                    if (start_search_label_index != -1)
                    {
                        notes_text = notes_text.Remove(start_search_label_index, html.Length);
                        notes_text = notes_text.Insert(start_search_label_index, html_current);
                        start_search_label_index += Searcher.Text.Length;
                        note_label.Text = notes_text;

                        note_label.IsVisible = false;
                        note_editor.IsVisible = true;
                        note_editor.IsVisible = false;
                        note_label.IsVisible = true;
                    }
                    else
                    {
                        try
                        {
                            CrossToastPopUp.Current.ShowToastMessage("Nothing was found");
                        }
                        catch
                        {
                            await DisplayAlert("Pop up error", "Try to downgrade 'minSdkVersion' to 16 and 'targetSdkVersion' to 28 OR use 'DisplayAlert'", "Got it!");
                        }
                        Connections.start_search_index = 0;
                        start_search_label_index = 0;
                    }

                    Connections.start_search_index = index_of_first_word + Searcher.Text.Length;
                }
                else
                {
                    try
                    {
                        CrossToastPopUp.Current.ShowToastMessage("Nothing was found");
                    }
                    catch
                    {
                        await DisplayAlert("Pop up error", "Try to downgrade 'minSdkVersion' to 16 and 'targetSdkVersion' to 28 OR use 'DisplayAlert'", "Got it!");
                    }
                    Connections.start_search_index = 0;
                    start_search_label_index = 0;
                }
            }
            else
            {
                await DisplayAlert("Warning", "Write some text.", "Ok");
            }
        }
    }
}