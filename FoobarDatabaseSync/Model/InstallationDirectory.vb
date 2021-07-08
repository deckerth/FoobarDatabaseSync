Imports Windows.Storage
Imports Windows.UI.Popups

Public Class InstallationDirectory

    Public Shared Current As InstallationDirectory

    Public Property Installations As New ObservableCollection(Of Installation)


    Private ContentLoaded As Boolean

    Public Sub New()
        Current = Me
    End Sub

    Const NameField As String = "Name"
    Const IsNASField As String = "IsNAS"
    Const PCNameField As String = "PCName"
    Const FoobarPathField As String = "FoobarPath"
    Const FoobarPathMRUTokenField As String = "FoobarPathMRUToken"
    Const ImagePathMRUTokenField As String = "ImagePathMRUToken"
    Const ImageFileNameField As String = "ImageFileName"
    Const InstallationsContainerName As String = "InstallationList"

#Region "Load"
    Public Async Function LoadAsync() As Task

        Dim localSettings = Windows.Storage.ApplicationData.Current.LocalSettings
        Dim playerList = localSettings.CreateContainer(InstallationsContainerName, Windows.Storage.ApplicationDataCreateDisposition.Always)

        Installations.Clear()

        For Each item In playerList.Values
            Dim newInst As New Installation
            Dim installationComposite As ApplicationDataCompositeValue = item.Value

            Try
                newInst.Name = installationComposite(NameField)
                newInst.IsNAS = installationComposite(IsNASField)
                newInst.PCName = installationComposite(PCNameField)
                newInst.FoobarPath = installationComposite(FoobarPathField)
                newInst.FoobarPathMruToken = installationComposite(FoobarPathMRUTokenField)
                newInst.ImagePathMruToken = installationComposite(ImagePathMRUTokenField)
                newInst.ImageFileName = installationComposite(ImageFileNameField)
            Catch ex As Exception
                Continue For
            End Try
            If newInst.Name Is Nothing Then
                Continue For
            End If

            Await newInst.LoadImageAsync()
            Installations.Add(newInst)
        Next

        ContentLoaded = True
    End Function

#End Region

#Region "SetupInstallations"
    Public Function GetInstallation(theName As String) As Installation

        Dim matches = Installations.Where(Function(otherPlayer) otherPlayer.Name.Equals(theName))
        If matches.Count() = 1 Then
            Return matches.First()
        End If

    End Function

    Public Sub AddInstallation(newInst As Installation)

        'Dim newInst As New Installation With {.Name = newName}

        If newInst Is Nothing OrElse String.IsNullOrEmpty(newInst.Name) Then
            Return
        End If

        Installations.Add(newInst)

        Dim localSettings = Windows.Storage.ApplicationData.Current.LocalSettings
        Dim installationList = localSettings.CreateContainer(InstallationsContainerName, Windows.Storage.ApplicationDataCreateDisposition.Always)
        Dim installationComposite = New Windows.Storage.ApplicationDataCompositeValue()
        installationComposite(NameField) = newInst.Name
        installationComposite(IsNASField) = newInst.IsNAS
        If Not String.IsNullOrEmpty(newInst.PCName) Then
            installationComposite(PCNameField) = newInst.PCName
        Else
            installationComposite(PCNameField) = newInst.Name
        End If
        If newInst.FoobarPath IsNot Nothing Then
            installationComposite(FoobarPathField) = newInst.FoobarPath
        End If
        If newInst.FoobarPathMruToken IsNot Nothing Then
            installationComposite(FoobarPathMRUTokenField) = newInst.FoobarPathMruToken
        End If
        If newInst.ImagePathMruToken IsNot Nothing Then
            installationComposite(ImagePathMRUTokenField) = newInst.ImagePathMruToken
        End If
        If newInst.ImageFileName IsNot Nothing Then
            installationComposite(ImageFileNameField) = newInst.ImageFileName
        End If

        installationList.Values(Guid.NewGuid().ToString()) = installationComposite
    End Sub

    Public Sub DeleteInstallation(theName As String)

        Dim toDelete As Installation = GetInstallation(theName)

        If toDelete IsNot Nothing Then
            Installations.Remove(toDelete)
        End If

        Dim localSettings = Windows.Storage.ApplicationData.Current.LocalSettings
        Dim installationList = localSettings.CreateContainer(InstallationsContainerName, Windows.Storage.ApplicationDataCreateDisposition.Always)
        Dim index As String
        For Each item In installationList.Values
            Dim installationComposite As ApplicationDataCompositeValue = item.Value
            If installationComposite(NameField).Equals(toDelete.Name) Then
                index = item.Key
                Exit For
            End If
        Next
        If index IsNot Nothing Then
            installationList.Values.Remove(index)
        End If

    End Sub

    Public Sub UpdateInstallation(theInst As Installation,
                                   Optional IsNAS As Nullable(Of Boolean) = Nothing,
                                   Optional newName As String = Nothing,
                                   Optional newPCName As String = Nothing,
                                   Optional FoobarPath As String = Nothing,
                                   Optional FoobarPathMRUToken As String = Nothing,
                                   Optional ImagePathMRUToken As String = Nothing,
                                   Optional ImageFileName As String = Nothing)

        Dim localSettings = Windows.Storage.ApplicationData.Current.LocalSettings
        Dim installationList = localSettings.CreateContainer(InstallationsContainerName, Windows.Storage.ApplicationDataCreateDisposition.Always)

        For Each item In installationList.Values
            Dim installationComposite As ApplicationDataCompositeValue = item.Value
            Dim currentName As String
            Try
                currentName = installationComposite(NameField)
            Catch ex As Exception
                Continue For
            End Try
            If currentName.Equals(theInst.Name) Then
                Dim newInstComposite As Windows.Storage.ApplicationDataCompositeValue = New Windows.Storage.ApplicationDataCompositeValue()

                If newName Is Nothing Then
                    newInstComposite(NameField) = theInst.Name
                Else
                    newInstComposite(NameField) = newName
                    theInst.Name = newName
                End If

                If newPCName Is Nothing Then
                    newInstComposite(PCNameField) = theInst.PCName
                Else
                    newInstComposite(PCNameField) = newPCName
                    theInst.PCName = newPCName
                End If

                If IsNAS IsNot Nothing Then
                    newInstComposite(IsNASField) = IsNAS
                    theInst.IsNAS = IsNAS
                ElseIf theInst.IsNAS IsNot Nothing Then
                    newInstComposite(IsNASField) = theInst.IsNAS
                End If

                If FoobarPath IsNot Nothing Then
                    newInstComposite(FoobarPathField) = FoobarPath
                    theInst.FoobarPath = FoobarPath
                ElseIf theInst.FoobarPath IsNot Nothing Then
                    newInstComposite(FoobarPathField) = theInst.FoobarPath
                End If

                If FoobarPathMRUToken IsNot Nothing Then
                    newInstComposite(FoobarPathMRUTokenField) = FoobarPathMRUToken
                    theInst.FoobarPathMruToken = FoobarPathMRUToken
                ElseIf theInst.FoobarPathMruToken IsNot Nothing Then
                    newInstComposite(FoobarPathMRUTokenField) = theInst.FoobarPathMruToken
                End If

                If ImagePathMRUToken IsNot Nothing Then
                    newInstComposite(ImagePathMRUTokenField) = ImagePathMRUToken
                    theInst.ImagePathMruToken = ImagePathMRUToken
                ElseIf theInst.ImagePathMruToken IsNot Nothing Then
                    newInstComposite(ImagePathMRUTokenField) = ImagePathMRUToken
                End If

                If ImageFileName IsNot Nothing Then
                    newInstComposite(ImageFileNameField) = ImageFileName
                    theInst.ImageFileName = ImageFileName
                ElseIf theInst.ImageFileName IsNot Nothing Then
                    newInstComposite(ImageFileNameField) = theInst.ImageFileName
                End If

                installationList.Values.Remove(item.Key)
                installationList.Values(item.Key) = newInstComposite
                Exit For
            End If
        Next

    End Sub

#End Region

#Region "CheckInstallations"

    Public Async Function CheckInstallationsAsync(selectedItems As IList(Of Installation)) As Task

        For Each item In selectedItems
            Await item.CheckVersionAsync()
        Next

        Dim MaxDate As DateTime = DateTime.MinValue

        For Each item In selectedItems
            If item.VersionDateTime.Equals(DateTime.MinValue) Then
                Continue For
            End If
            Dim diff = item.VersionDateTime - MaxDate
            If diff.TotalSeconds > 1 Then
                MaxDate = item.VersionDateTime
            End If
        Next

        If MaxDate.Equals(DateTime.MinValue) Then
            Return
        End If

        For Each item In selectedItems
            If item.VersionDateTime.Equals(DateTime.MinValue) Then
                item.State = Installation.InstallationState.Offline
                Continue For
            End If
            Dim diff = item.VersionDateTime - MaxDate
            If diff.TotalSeconds < -5 Then
                item.State = Installation.InstallationState.Outdated
            Else
                item.State = Installation.InstallationState.Current
            End If
        Next
    End Function

#End Region

#Region "CopyDB"

    Private Async Function CopyFolderRecursiveAsync(srcFolder As StorageFolder, destParentFolder As StorageFolder) As Task(Of Boolean)
        If srcFolder Is Nothing OrElse destParentFolder Is Nothing Then
            Return False
        End If
        Try
            Dim destFolder As StorageFolder
            Try
                destFolder = Await destParentFolder.GetFolderAsync(srcFolder.Name)
                If destFolder IsNot Nothing Then
                    Await destFolder.DeleteAsync()
                End If
            Catch ex As Exception
                ' Folder does not exist, OK.
            End Try
            destFolder = Await destParentFolder.CreateFolderAsync(srcFolder.Name)
            Dim folders = Await srcFolder.GetFoldersAsync()
            If folders IsNot Nothing Then
                For Each folder In folders
                    Await CopyFolderRecursiveAsync(folder, destFolder)
                Next
            End If
            Dim files = Await srcFolder.GetFilesAsync()
            If files IsNot Nothing Then
                For Each file In files
                    Await file.CopyAsync(destFolder)
                Next
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function


    Public Async Function CopyDatabase(source As Installation, dest As Installation) As Task

        Dim ok As Boolean

        If String.IsNullOrEmpty(source.FoobarPathMruToken) OrElse String.IsNullOrEmpty(dest.FoobarPathMruToken) Then
            Return
        End If
        Try
            Dim destFolder = Await AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(dest.FoobarPathMruToken)
            Dim srcFolder = Await AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(source.FoobarPathMruToken)
            If srcFolder IsNot Nothing Then
                Dim toCopy As StorageFolder
                toCopy = Await srcFolder.GetFolderAsync("library")
                ok = Await CopyFolderRecursiveAsync(toCopy, destFolder)
                toCopy = Await srcFolder.GetFolderAsync("index-data")
                ok = ok AndAlso Await CopyFolderRecursiveAsync(toCopy, destFolder)
            End If
        Catch ex As Exception
        End Try

        If Not ok Then
            Dim msg As New MessageDialog("Fehler beim Kopieren von " + source.Name + " nach " + dest.Name)
            Await msg.ShowAsync()
        End If

    End Function

#End Region

End Class
