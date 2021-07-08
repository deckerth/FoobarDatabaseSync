' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

Imports System.Net
Imports Windows.Networking
Imports Windows.UI.Popups
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Public Sub New()
        InitializeComponent()
        InstallationGrid.ItemsSource = InstallationDirectory.Current.Installations
        Dim computername = System.Net.Dns.GetHostName()
        Dim index As Integer
        For Each item In InstallationDirectory.Current.Installations
            If item.IsNAS OrElse item.PCName IsNot Nothing And item.PCName.Equals(computername) Then
                InstallationGrid.SelectRange(New ItemIndexRange(index, 1))
            End If
            index = index + 1
        Next
    End Sub

    Private Sub ConfigureSystems_Click(sender As Object, e As RoutedEventArgs) Handles ConfigureSystems.Click
        Me.Frame.Navigate(GetType(SetupSystems))
    End Sub

    Private Sub DisableControls()
        ProgressRing.IsActive = True
        SelectAll.IsEnabled = False
        DeselectAll.IsEnabled = False
        CheckInstallations.IsEnabled = False
        ConfigureSystems.IsEnabled = False
        Sync.IsEnabled = False
    End Sub

    Private Sub EnableControls()
        ProgressRing.IsActive = False
        SelectAll.IsEnabled = True
        DeselectAll.IsEnabled = True
        CheckInstallations.IsEnabled = True
        ConfigureSystems.IsEnabled = True
        Sync.IsEnabled = True
    End Sub

    Private Async Function CheckInstallations_ClickAsync(sender As Object, e As RoutedEventArgs) As Task Handles CheckInstallations.Click

        If InstallationGrid.SelectedItems.Count = 0 Then
            Dim msg As New MessageDialog("Es ist keine Installation ausgewählt.")
            Await msg.ShowAsync()
            Return
        End If

        Dim selectedItems As New List(Of Installation)
        For Each item In InstallationGrid.SelectedItems
            selectedItems.Add(item)
        Next
        DisableControls()
        Await InstallationDirectory.Current.CheckInstallationsAsync(selectedItems)
        EnableControls()

    End Function

    Private Sub SelectAll_Click(sender As Object, e As RoutedEventArgs) Handles SelectAll.Click
        InstallationGrid.SelectRange(New ItemIndexRange(0, InstallationDirectory.Current.Installations.Count))
    End Sub

    Private Sub DeselectAll_Click(sender As Object, e As RoutedEventArgs) Handles DeselectAll.Click
        InstallationGrid.DeselectRange(New ItemIndexRange(0, InstallationDirectory.Current.Installations.Count))
    End Sub

    Private Async Sub Sync_Click(sender As Object, e As RoutedEventArgs) Handles Sync.Click

        If InstallationGrid.SelectedItems.Count < 2 Then
            Dim msg As New MessageDialog("Es müssen mindestens zwei Installationen ausgewählt werden.")
            Await msg.ShowAsync()
            Return
        End If

        Dim selectedItems As New List(Of Installation)
        For Each item In InstallationGrid.SelectedItems
            selectedItems.Add(item)
        Next

        Dim chooser As New SyncDialog
        chooser.SetInstallations(selectedItems)
        Await chooser.ShowAsync()
        If Not chooser.DialogCancelled() Then
            Dim sourceInst = chooser.GetSourceInst()
            If sourceInst IsNot Nothing Then
                DisableControls()
                For Each item In selectedItems
                    If Not item.Equals(sourceInst) Then
                        Await InstallationDirectory.Current.CopyDatabase(sourceInst, item)
                    End If
                Next
                Await InstallationDirectory.Current.CheckInstallationsAsync(selectedItems)
                EnableControls()
            End If
        End If

    End Sub
End Class
