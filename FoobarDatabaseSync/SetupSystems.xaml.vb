' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class SetupSystems
    Inherits Page

    Public Sub New()
        InitializeComponent()
        InstallationGrid.ItemsSource = InstallationDirectory.Current.Installations
    End Sub

    Private Sub GoBack_Click(sender As Object, e As RoutedEventArgs) Handles GoBack.Click
        Me.Frame.GoBack()
    End Sub

    Private Async Sub AddInstallation_ClickAsync(sender As Object, e As RoutedEventArgs) Handles AddInstallation.Click
        Dim editor = New InstallationEditor
        editor.SetAction(InstallationEditor.Actions.NewInstallation)
        Await editor.ShowAsync()
        If Not editor.DialogCancelled() Then
            Dim inst = editor.GetInstallation()
            Await inst.LoadImageAsync()
            InstallationDirectory.Current.AddInstallation(inst)
        End If
    End Sub

    Private Async Sub OnInstallationSelected(sender As Object, e As ItemClickEventArgs)
        Dim inst As Installation = DirectCast(e.ClickedItem, Installation)
        Dim editor = New InstallationEditor
        editor.SetAction(InstallationEditor.Actions.Edit)
        editor.SetInstallation(inst)
        Await editor.ShowAsync()
        If Not editor.DialogCancelled() Then
            Dim reworkedInst = editor.GetInstallation()
            InstallationDirectory.Current.UpdateInstallation(inst,
                                                             IsNAS:=reworkedInst.IsNAS,
                                                             newName:=reworkedInst.Name,
                                                             newPCName:=reworkedInst.PCName,
                                                             FoobarPath:=reworkedInst.FoobarPath,
                                                             FoobarPathMRUToken:=reworkedInst.FoobarPathMruToken,
                                                             ImagePathMRUToken:=reworkedInst.ImagePathMruToken,
                                                             ImageFileName:=reworkedInst.ImageFileName)
            Await inst.LoadImageAsync()
        End If
    End Sub

    Private Sub DeleteInstallation_Click(sender As Object, e As RoutedEventArgs)

        Dim selectedInst As String = DirectCast(sender, AppBarButton).CommandParameter
        If selectedInst Is Nothing Then
            Return
        End If

        InstallationDirectory.Current.DeleteInstallation(selectedInst)

    End Sub
End Class
