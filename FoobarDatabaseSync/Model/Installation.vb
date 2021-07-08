Imports System.Text
Imports Windows.Globalization.DateTimeFormatting
Imports Windows.Storage

Public Class Installation
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Protected Overridable Sub OnPropertyChanged(ByVal PropertyName As String)
        ' Raise the event, and make this procedure
        ' overridable, should someone want to inherit from
        ' this class and override this behavior:
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(PropertyName))
    End Sub

    Enum InstallationState
        Undefined
        NotChecked
        Offline
        Current
        Outdated
    End Enum

    Private _Name As String
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            If _Name Is Nothing OrElse Not _Name.Equals(value) Then
                _Name = value
                OnPropertyChanged("Name")
            End If
        End Set
    End Property

    Public Property PCName As String

    Private _IsNAS As Nullable(Of Boolean)
    Public Property IsNAS As Nullable(Of Boolean)
        Get
            Return _IsNAS
        End Get
        Set(value As Nullable(Of Boolean))
            If _IsNAS Is Nothing OrElse _IsNAS <> value Then
                _IsNAS = value
                OnPropertyChanged("IsNAS")
            End If
        End Set
    End Property

    Private _FoobarPath As String
    Public Property FoobarPath As String
        Get
            Return _FoobarPath
        End Get
        Set(value As String)
            If _FoobarPath Is Nothing OrElse Not _FoobarPath.Equals(value) Then
                _FoobarPath = value
                OnPropertyChanged("FoobarPath")
            End If
        End Set
    End Property

    Public Property FoobarPathMruToken As String
    Public Property ImagePathMruToken As String
    Public Property ImageFileName As String
    Public Property Image As BitmapImage

    Private _State As InstallationState = InstallationState.Undefined

    Public Property State As InstallationState
        Get
            Return _State
        End Get
        Set(value As InstallationState)
            If value <> State Then
                _State = value
                Select Case _State
                    Case InstallationState.Current
                        StateString = "Aktuell"
                        StateColor = New SolidColorBrush(Windows.UI.Colors.Green)
                    Case InstallationState.Outdated
                        StateString = "Nicht aktuell"
                        StateColor = New SolidColorBrush(Windows.UI.Colors.Red)
                    Case InstallationState.Offline
                        StateString = "Offline"
                        StateColor = New SolidColorBrush(Windows.UI.Colors.Gray)
                    Case InstallationState.NotChecked
                        StateString = "Nicht geprüft"
                        StateColor = New SolidColorBrush(Windows.UI.Colors.Gray)
                End Select
                OnPropertyChanged("StateString")
                OnPropertyChanged("StateColor")
            End If
        End Set
    End Property

    Public Sub New()
        State = InstallationState.NotChecked
    End Sub

    Public Async Function LoadImageAsync() As Task
        Dim tempFolder = Windows.Storage.ApplicationData.Current.LocalFolder
        Dim imageFile As StorageFile

        If String.IsNullOrEmpty(ImageFileName) Then
            Return
        End If

        Try
            imageFile = Await tempFolder.GetFileAsync(Me.ImageFileName)
        Catch ex As Exception
        End Try

        If imageFile Is Nothing Then
            ' Local copy does not exist, create a local copy
            If String.IsNullOrEmpty(ImagePathMruToken) Then
                Return
            Else
                Try
                    Dim origImageFile As StorageFile
                    origImageFile = Await AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(ImagePathMruToken)
                    If origImageFile Is Nothing Then
                        Return
                    End If
                    Await origImageFile.CopyAsync(tempFolder, origImageFile.Name, NameCollisionOption.ReplaceExisting)
                    imageFile = Await tempFolder.GetFileAsync(origImageFile.Name)
                Catch ex As Exception
                    Return
                End Try
            End If
        End If

        If imageFile Is Nothing Then
            Return
        End If

        Try
            ' Open a stream for the selected file.
            Dim fileStream = Await imageFile.OpenAsync(Windows.Storage.FileAccessMode.Read)
            ' Set the image source to the selected bitmap.
            Image = New Windows.UI.Xaml.Media.Imaging.BitmapImage()
            Await Image.SetSourceAsync(fileStream)
            OnPropertyChanged("Image")
            fileStream.Dispose()
        Catch ex As Exception
        End Try

    End Function

    Public Property StateString As String
    Public Property StateColor As Brush

    Private _VersionDateTime As DateTime
    Public Property VersionDateTime As DateTime
        Get
            Return _VersionDateTime
        End Get
        Set(value As DateTime)
            If value <> _VersionDateTime Then
                _VersionDateTime = value
                VersionDateTimeString = ConvertToDateTimeStr(_VersionDateTime)
                OnPropertyChanged("VersionDateTimeString")
            End If

        End Set
    End Property
    Public Property VersionDateTimeString As String

    Public Property AddButtonVisibility As Visibility
    Public Property DeleteButtonVisibility As Visibility

    Public Async Function CheckVersionAsync() As Task
        If String.IsNullOrEmpty(FoobarPathMruToken) Then
            Return
        End If
        Try
            Dim foobarFolder = Await AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(FoobarPathMruToken)
            If foobarFolder IsNot Nothing Then
                Dim libFolder = Await foobarFolder.GetFolderAsync("library")
                If libFolder IsNot Nothing Then
                    Dim filtersFile = Await libFolder.GetFileAsync("filters")
                    If filtersFile IsNot Nothing Then
                        Dim properties = Await filtersFile.GetBasicPropertiesAsync()
                        If properties IsNot Nothing Then
                            Me.VersionDateTime = properties.ItemDate.DateTime
                        End If
                    End If
                End If
            End If
        Catch ex As Exception

        End Try
    End Function
#Region "Date conversion"
    Public Shared Function ConvertToDateTimeStr(aDate As DateTime) As String
        Return DateTimeFormatter.ShortDate.Format(aDate) + " " + DateTimeFormatter.ShortTime.Format(aDate)
    End Function

    Public Shared Function ConvertToDate(ByRef datestr As String) As DateTime

        Try
            ' Create two different encodings.
            Dim ascii As Encoding = Encoding.GetEncoding("US-ASCII")
            Dim unicode As Encoding = Encoding.Unicode

            ' Convert the string into a byte array.
            Dim unicodeBytes As Byte() = unicode.GetBytes(datestr)

            ' Perform the conversion from one encoding to the other.
            Dim asciiBytes As Byte() = Encoding.Convert(unicode, ascii, unicodeBytes)

            ' Convert the new byte array into a char array and then into a string.
            Dim asciiChars(ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length) - 1) As Char
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0)
            Dim asciiString As New String(asciiChars)

            Return DateTime.Parse(asciiString.Replace("?", ""))
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

#End Region

End Class
