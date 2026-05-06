using System.Drawing;

namespace MEditApp;

public partial class MEditForm : Form
{
    private ToolStrip toolStrip = null!;
    private MenuStrip menuStrip = null!;
    private RichTextBox textEditor = null!;
    private string currentFileName = string.Empty;

    public MEditForm()
    {
        InitializeComponent();
        InitializeMenuStrip();
        InitializeToolStrip();
        InitializeEditor();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MEdit - Новый документ";
        ResumeLayout(false);
    }

    private void InitializeToolStrip()
    {
        toolStrip = new ToolStrip
        {
            Dock = DockStyle.Top,
            BackColor = Color.LightGray,
            GripStyle = ToolStripGripStyle.Hidden
        };

        ToolStripButton btnNew = CreateToolStripButton("Создать", "Создать новый документ (Ctrl+N)", BtnNew_Click);
        ToolStripButton btnOpen = CreateToolStripButton("Открыть", "Открыть файл (Ctrl+O)", BtnOpen_Click);
        ToolStripButton btnSave = CreateToolStripButton("Сохранить", "Сохранить документ (Ctrl+S)", BtnSave_Click);
        ToolStripButton btnExit = CreateToolStripButton("Выход", "Выход из программы", BtnExit_Click);

        toolStrip.Items.Add(btnNew);
        toolStrip.Items.Add(btnOpen);
        toolStrip.Items.Add(btnSave);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(btnExit);

        Controls.Add(toolStrip);
    }

    private static ToolStripButton CreateToolStripButton(string text, string tooltip, EventHandler clickHandler)
    {
        ToolStripButton button = new()
        {
            Text = text,
            ToolTipText = tooltip,
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            ImageTransparentColor = Color.Magenta
        };
        button.Click += clickHandler;
        return button;
    }

    private void InitializeMenuStrip()
    {
        menuStrip = new MenuStrip();

        ToolStripMenuItem fileMenu = new("Файл");
        fileMenu.DropDownItems.Add(CreateMenuItem("Новый", Keys.Control | Keys.N, MnuNew_Click));
        fileMenu.DropDownItems.Add(CreateMenuItem("Открыть", Keys.Control | Keys.O, MnuOpen_Click));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(CreateMenuItem("Сохранить", Keys.Control | Keys.S, MnuSave_Click));
        fileMenu.DropDownItems.Add(CreateMenuItem("Сохранить как...", Keys.Control | Keys.Shift | Keys.S, MnuSaveAs_Click));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(CreateMenuItem("Выход", Keys.None, MnuExit_Click));
        menuStrip.Items.Add(fileMenu);

        ToolStripMenuItem editMenu = new("Правка");
        editMenu.DropDownItems.Add(CreateMenuItem("Вырезать", Keys.Control | Keys.X, (_, _) => textEditor.Cut()));
        editMenu.DropDownItems.Add(CreateMenuItem("Копировать", Keys.Control | Keys.C, (_, _) => textEditor.Copy()));
        editMenu.DropDownItems.Add(CreateMenuItem("Вставить", Keys.Control | Keys.V, (_, _) => textEditor.Paste()));
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(CreateMenuItem("Выделить всё", Keys.Control | Keys.A, (_, _) => textEditor.SelectAll()));
        menuStrip.Items.Add(editMenu);

        MainMenuStrip = menuStrip;
        Controls.Add(menuStrip);
    }

    private static ToolStripMenuItem CreateMenuItem(string text, Keys shortcutKeys, EventHandler clickHandler)
    {
        ToolStripMenuItem menuItem = new(text)
        {
            ShortcutKeys = shortcutKeys
        };
        menuItem.Click += clickHandler;
        return menuItem;
    }

    private void InitializeEditor()
    {
        textEditor = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Arial", 12F),
            AcceptsTab = true,
            BorderStyle = BorderStyle.FixedSingle,
            WordWrap = true
        };
        Controls.Add(textEditor);
        Controls.SetChildIndex(menuStrip, 0);
        Controls.SetChildIndex(toolStrip, 1);
        Controls.SetChildIndex(textEditor, 2);
    }

    private void BtnNew_Click(object? sender, EventArgs e)
    {
        CreateNewDocument(sender, e);
    }

    private void BtnOpen_Click(object? sender, EventArgs e)
    {
        OpenDocument();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        SaveDocument(false);
    }

    private void BtnExit_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void MnuNew_Click(object? sender, EventArgs e)
    {
        CreateNewDocument(sender, e);
    }

    private void MnuOpen_Click(object? sender, EventArgs e)
    {
        OpenDocument();
    }

    private void MnuSave_Click(object? sender, EventArgs e)
    {
        SaveDocument(false);
    }

    private void MnuSaveAs_Click(object? sender, EventArgs e)
    {
        SaveDocument(true);
    }

    private void MnuExit_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void CreateNewDocument(object? sender, EventArgs e)
    {
        if (!ConfirmSaveCurrentDocument(sender, e))
        {
            return;
        }

        textEditor.Clear();
        currentFileName = string.Empty;
        Text = "MEdit - Новый документ";
    }

    private void OpenDocument()
    {
        if (!ConfirmSaveCurrentDocument(this, EventArgs.Empty))
        {
            return;
        }

        using OpenFileDialog openFileDialog = new()
        {
            Title = "Открыть файл",
            Filter = "Текстовые файлы (*.txt)|*.txt|RTF файлы (*.rtf)|*.rtf|Все файлы (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (openFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (Path.GetExtension(openFileDialog.FileName).Equals(".rtf", StringComparison.OrdinalIgnoreCase))
            {
                textEditor.LoadFile(openFileDialog.FileName, RichTextBoxStreamType.RichText);
            }
            else
            {
                textEditor.Text = File.ReadAllText(openFileDialog.FileName);
            }

            currentFileName = openFileDialog.FileName;
            Text = $"MEdit - {currentFileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Ошибка открытия файла: " + ex.Message, "MEdit", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool SaveDocument(bool saveAs)
    {
        if (!saveAs && !string.IsNullOrWhiteSpace(currentFileName))
        {
            return SaveToFile(currentFileName);
        }

        using SaveFileDialog saveFileDialog = new()
        {
            Title = "Сохранить файл",
            Filter = "Текстовые файлы (*.txt)|*.txt|RTF файлы (*.rtf)|*.rtf|Все файлы (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            FileName = string.IsNullOrWhiteSpace(currentFileName) ? "Новый документ.txt" : Path.GetFileName(currentFileName)
        };

        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        return SaveToFile(saveFileDialog.FileName);
    }

    private bool SaveToFile(string fileName)
    {
        try
        {
            if (Path.GetExtension(fileName).Equals(".rtf", StringComparison.OrdinalIgnoreCase))
            {
                textEditor.SaveFile(fileName, RichTextBoxStreamType.RichText);
            }
            else
            {
                File.WriteAllText(fileName, textEditor.Text);
            }

            currentFileName = fileName;
            Text = $"MEdit - {currentFileName}";
            MessageBox.Show(this, "Файл успешно сохранён!", "MEdit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Ошибка сохранения файла: " + ex.Message, "MEdit", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private bool ConfirmSaveCurrentDocument(object? sender, EventArgs e)
    {
        if (textEditor.TextLength == 0)
        {
            return true;
        }

        DialogResult result = MessageBox.Show(
            this,
            "Сохранить текущий документ?",
            "MEdit",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (result == DialogResult.Cancel)
        {
            return false;
        }

        return result != DialogResult.Yes || SaveDocument(false);
    }
}
