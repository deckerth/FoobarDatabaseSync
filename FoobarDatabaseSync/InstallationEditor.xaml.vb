' Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports Windows.Storage
Imports Windows.Storage.Pickers

Public NotInheritable Class InstallationEditor
    Inherits ContentDialog

    Class StringContainer
        Public content As New String("")
    End Class

    Public Enum Actions
        Edit
        NewInstallation
    End Enum

    Private oldInstallation As Installation
    Private cancelled As Boolean
    Private intendedAction As Actions
    Private foobarPathMRUToken As String
    Private imagePathMRUToken As String
    Private imageFileName As String

    Public Sub SetInstallation(ByRef inst As Installation)
        If inst Is Nothing Then
            oldInstallation = New Installation
            Return
        End If

        If inst.Name IsNot Nothing Then
            InstallationName.Text = inst.Name
        End If
        If inst.PCName IsNot Nothing Then
            PCName.Text = inst.PCName
        End If
        IsNASCheckbox.IsChecked = (inst.IsNAS IsNot Nothing AndAlso inst.IsNAS)
        If inst.FoobarPath IsNot Nothing Then
            FoobarPathBox.Text = inst.FoobarPath
        End If
        If inst.FoobarPathMruToken IsNot Nothing Then
            foobarPathMRUToken = inst.FoobarPathMruToken
        End If
        If inst.ImageFileName IsNot Nothing Then
            imageFileName = inst.ImageFileName
        End If
        If inst.ImagePathMruToken IsNot Nothing Then
            imagePathMRUToken = inst.ImagePathMruToken
        End If
        If inst.Image IsNot Nothing Then
            InstallationImage.Source = inst.Image
        End If
        oldInstallation = inst
        CheckInput()
    End Sub

    Public Sub SetAction(ByRef action As Actions)
        intendedAction = action
        cancelled = True
    End Sub

    Public Function DialogCancelled() As Boolean
        Return cancelled
    End Function

    Public Function GetInstallation() As Installation
        Dim inst = New Installation With {
            .Name = InstallationName.Text,
            .PCName = PCName.Text,
            .IsNAS = IsNASCheckbox.IsChecked,
            .FoobarPath = FoobarPathBox.Text,
            .FoobarPathMruToken = foobarPathMRUToken,
            .ImageFileName = imageFileName,
            .ImagePathMruToken = imagePathMRUToken
        }
        Return inst
    End Function

    Private Function NameIsValid(Optional ByRef errorMessage As StringContainer = Nothing) As Boolean

        If InstallationName.Text Is Nothing OrElse InstallationName.Text.Trim().Equals("") Then
            If errorMessage IsNot Nothing Then
                errorMessage.content = "Bitte einen Namen eingeben."
            End If
            Return False
        End If

        Dim currentName As String = InstallationName.Text.Trim()

        If intendedAction = Actions.NewInstallation OrElse Not oldInstallation.Name.Equals(currentName) Then
            ' The existence check is skipped when renaming, and if the name has not been changed
            If InstallationDirectory.Current.GetInstallation(currentName) IsNot Nothing Then
                If errorMessage IsNot Nothing Then
                    errorMessage.content = "Eine Installation mit dem Namen " + currentName + " ist bereits definiert."
                End If
                Return False
            End If
        End If

        If errorMessage IsNot Nothing Then
            errorMessage.content = ""
        End If
        Return True

    End Function

    Private Sub CheckInput()

        Dim errorMessage As New StringContainer()

        If NameIsValid(errorMessage) Then
            InstallationName.SetValue(BorderBrushProperty, New SolidColorBrush(Windows.UI.Colors.Black))
            InstallationEditor.IsPrimaryButtonEnabled = True
        Else
            InstallationName.SetValue(BorderBrushProperty, New SolidColorBrush(Windows.UI.Colors.Red))
            InstallationEditor.IsPrimaryButtonEnabled = False
        End If

        ErrorMessageDisplay.Text = errorMessage.content

    End Sub

    Private Sub InstallationEditor_PrimaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        InstallationEditor.Hide()
        cancelled = False
    End Sub

    Private Sub InstallationEditor_SecondaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        InstallationEditor.Hide()
        cancelled = True
    End Sub

    Private Async Sub PickFoobarPath_Click(sender As Object, e As RoutedEventArgs)
        Dim folderPicker = New FolderPicker With {
            .SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            .ViewMode = PickerViewMode.Thumbnail,
            .CommitButtonText = "Installationsorder auswählen"
        }

        folderPicker.FileTypeFilter.Clear()
        folderPicker.FileTypeFilter.Add("*")

        Dim selectedFolder As StorageFolder = Await folderPicker.PickSingleFolderAsync()
        If selectedFolder IsNot Nothing Then
            FoobarPathBox.Text = selectedFolder.Path
            foobarPathMRUToken = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(selectedFolder)
        End If
    End Sub

    Private Async Sub LoadInstallationImage_Click(sender As Object, e As RoutedEventArgs)
        Dim openPicker = New Windows.Storage.Pickers.FileOpenPicker With {
            .SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
            .ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
        }

        ' Filter to include a sample subset of file types.
        openPicker.FileTypeFilter.Clear()
        openPicker.FileTypeFilter.Add(".png")
        openPicker.FileTypeFilter.Add(".jpg")

        ' Open the file picker.
        Dim file = Await openPicker.PickSingleFileAsync()

        ' file is null if user cancels the file picker.
        If file IsNot Nothing Then
            Try
                Await LoadImageAsync(file)
                imageFileName = file.Name
                imagePathMRUToken = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file)
            Catch ex As Exception
            End Try
        End If

    End Sub

    Private Async Function LoadImageAsync(ByVal imageFile As Windows.Storage.StorageFile) As Task

        If imageFile IsNot Nothing Then
            Try
                ' Open a stream for the selected file.
                Dim fileStream = Await imageFile.OpenAsync(Windows.Storage.FileAccessMode.Read)

                ' Set the image source to the selected bitmap.
                Dim BitmapImage = New Windows.UI.Xaml.Media.Imaging.BitmapImage()

                BitmapImage.SetSource(fileStream)
                InstallationImage.Source = BitmapImage
            Catch ex As Exception
            End Try
        End If

    End Function

    Private Sub InstallationName_TextChanged(sender As Object, e As TextChangedEventArgs) Handles InstallationName.TextChanged
        CheckInput()
    End Sub
End Class
