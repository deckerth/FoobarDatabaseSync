' Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Public NotInheritable Class SyncDialog
    Inherits ContentDialog

    Dim cancelled As Boolean
    Dim InstallationList As New List(Of String)

    Public Function DialogCancelled() As Boolean
        Return cancelled
    End Function

    Public Sub SetInstallations(installations As List(Of Installation))
        InstallationList.Clear()
        For Each item In installations
            InstallationList.Add(item.Name)
        Next
        SourceSystem.ItemsSource = InstallationList
    End Sub

    Public Function GetSourceInst() As Installation
        Return InstallationDirectory.Current.GetInstallation(SourceSystem.SelectedItem)
    End Function


    Private Sub ContentDialog_PrimaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        cancelled = False
        SyncDialog.Hide()
    End Sub

    Private Sub ContentDialog_SecondaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        cancelled = True
        SyncDialog.Hide()
    End Sub
End Class
