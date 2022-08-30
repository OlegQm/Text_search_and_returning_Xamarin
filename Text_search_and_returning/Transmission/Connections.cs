namespace Text_search_and_returning
{
    // (RUS) Этот класс был создан для передачи данных из класса 'TextEditing' в классы 'EditorRendererExtended' и 'KeyboardHelper', и наоборот
    // (ENG) This class was created to pass data from the 'TextEditing' class to the 'EditorRendererExtended' and 'KeyboardHelper' classes, and vice versa
    public class Connections
    {
        public static string searcher_text_transmission { get; set; }
        public static int start_search_index { get; set; }
    }
}