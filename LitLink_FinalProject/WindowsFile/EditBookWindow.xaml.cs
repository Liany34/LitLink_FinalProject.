using Microsoft.Win32;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LitLink_FinalProject.WindowsFile
{
    /// <summary>
    /// Interaction logic for EditBookWindow.xaml
    /// </summary>
    public partial class EditBookWindow : Window
    {
        private Book _bookToEdit;
        private Apiservice _apiService = new Apiservice();

        // ה-Constructor מקבל את אובייקט הספר הנוכחי
        public EditBookWindow(Book currentBook)
        {
            InitializeComponent();
            this._bookToEdit = currentBook;

            // הפעלת תהליך הטעינה הדינמי מהמסד
            InitializeWindowData();
        }

        // פונקציית עזר אסינכרונית שמנהלת את סדר טעינת הנתונים
        private async void InitializeWindowData()
        {
            // 1. טעינת רשימת השפות הדינמית (ממתינים שה-Task יסתיים)
            await LoadLanguagesFromDatabaseAsync();

            // 2. רק אחרי שהשפות נטענו ל-UI, ממלאים את שאר נתוני הספר
            LoadBookData();
        }

        // תיקון שורה 50: הפיכת המתודה לאסינכרונית עם תמיכה ב-Task ו-await
        private async Task LoadLanguagesFromDatabaseAsync()
        {
            try
            {
                // שימוש ב-await כדי לחלץ את הרשימה מתוך ה-Task האסינכרוני
                var languagesList = await _apiService.GetAllLanguages();

                // קישור הרשימה לפקד ה-UI
                CmbLanguage.ItemsSource = languagesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load languages list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // מילוי שדות הטופס בערכים הקיימים של הספר
        private void LoadBookData()
        {
            TxtBookName.Text = _bookToEdit.BookName;
            TxtDescription.Text = _bookToEdit.Information;
            TxtPrice.Text = _bookToEdit.Price.ToString();
            TxtFilePath.Text = _bookToEdit.BookLink;
            TxtCoverPath.Text = _bookToEdit.Cover;
            DpPublishDate.SelectedDate = _bookToEdit.PublicationDate;

            // תיקון שורה 69: שינוי מ-Language ל-IdLanguage (או השם הקיים אצלך במודל Book)
            if (_bookToEdit.IdLanguage != null)
            {
                // שורה 71: שולחים את אובייקט השפה המשויך לספר
                SetSelectedLanguage(_bookToEdit.IdLanguage);
            }
        }

        // פונקציית עזר למציאת וסימון השפה הנוכחית ברשימה שנטענה מהמסד
        private void SetSelectedLanguage(Language currentLanguage)
        {
            if (currentLanguage == null || CmbLanguage.ItemsSource == null) return;

            foreach (Language lang in CmbLanguage.ItemsSource)
            {
                if (lang.Id == currentLanguage.Id)
                {
                    CmbLanguage.SelectedItem = lang;
                    break;
                }
            }
        }

        // לחיצה על כפתור שמירת השינויים
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 1. בדיקות תקינות קלט
            if (string.IsNullOrEmpty(TxtBookName.Text.Trim()) || string.IsNullOrEmpty(TxtPrice.Text.Trim()))
            {
                MessageBox.Show("Book Name and Price are required fields!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(TxtPrice.Text.Trim(), out double parsedPrice))
            {
                MessageBox.Show("Please enter a valid number for the price.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpPublishDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a valid publication date.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string filePath = TxtFilePath.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filePath) && !filePath.EndsWith(".epub"))
            {
                MessageBox.Show("LitLink only supports books in EPUB format! Please select a valid .epub file.",
                                "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbLanguage.SelectedItem == null)
            {
                MessageBox.Show("Please select a language for the book.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. עדכון האובייקט המקומי בערכים החדשים שנבחרו/הוקלדו
            _bookToEdit.BookName = TxtBookName.Text.Trim();
            _bookToEdit.Information = TxtDescription.Text.Trim();
            _bookToEdit.Price = parsedPrice;
            _bookToEdit.BookLink = TxtFilePath.Text.Trim();
            _bookToEdit.Cover = TxtCoverPath.Text.Trim();
            _bookToEdit.PublicationDate = DpPublishDate.SelectedDate.Value;

            // שליפת אובייקט השפה שנבחר ושמירתו בתוך אובייקט הספר
            Language selectedLang = CmbLanguage.SelectedItem as Language;
            if (selectedLang != null)
            {
                // תיקון שורה 141: השמה לתוך השדה הנכון במודל (IdLanguage)
                _bookToEdit.IdLanguage = selectedLang;
            }

            // 3. שמירה סופית למסד הנתונים באמצעות ה-ApiService
            try
            {
                _apiService.UpdateBook(_bookToEdit);

                MessageBox.Show("Book details updated successfully!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

                // סימון שהפעולה הצליחה וסגירת החלון
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save changes to the database: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // פתיחת סייר קבצים לבחירת קובץ הספר מהמחשב
        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // חסימה ויזואלית בסייר הקבצים - יציג רק קבצים שמסתיימים ב- .epub
            openFileDialog.Filter = "E-Books (*.epub)|*.epub";

            if (openFileDialog.ShowDialog() == true)
            {
                TxtFilePath.Text = openFileDialog.FileName;
            }
        }

        // ביטול וסגירה
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
