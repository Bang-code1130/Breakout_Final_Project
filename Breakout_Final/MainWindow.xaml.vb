Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Class MainWindow

    Private BGMusic As New MediaPlayer()

    Dim GameLoop As New DispatcherTimer
    Private BRICK_W As Integer = 100
    Private BRICK_H As Integer = 25
    Private BRICK_COLS As Integer = 7
    Private BRICK_RW As Integer = 6
    Private BRICK_GAP As Integer = 1

    Dim PADDLE_BUFFER As Integer = 15
    Dim Paddle As New Rectangle()
    Const PADDLE_DISTANCE_FROM_BOTTOM As Double = 50
    Dim CENTER_OF_PADDLE As Double
    Private PADDLE_SPEED As Double = 10
    Dim PADDLE_TRANSLATE As New TranslateTransform(0, 0)

    Private MOVE_LEFT As Boolean
    Private MOVE_RIGHT As Boolean

    Private BALL As New Ellipse()
    Private BALL_BUFFER As Double = 5
    Private BALL_TRANSLATE As New TranslateTransform(0, 0)
    Private BALL_SPEED_X As Double = 5
    Private BALL_SPEED_Y As Double = 5


    Private WALL_TOP As Double = 0
    Private WALL_LEFT As Double = 0
    Private WALL_RIGHT As Double
    Private WALL_BOTTOM As Double

    Sub New()
        InitializeComponent()

        Dim musicPath As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds/Background_Music.wav")
        BGMusic.Open(New Uri(musicPath))
        BGMusic.Volume = 1.0
        AddHandler BGMusic.MediaEnded, AddressOf LoopMusic
        BGMusic.Play()

        GameLoop.Interval = TimeSpan.FromMilliseconds(1)
        AddHandler GameLoop.Tick, AddressOf UpdateLoop

        DrawBricks()
        DrawPaddle()
        DrawBall()
        SETWALLS()
        GameLoop.Start()
    End Sub
    Private Sub UpdateLoop(sender As Object, e As EventArgs)
        MovePaddle()
        MoveBall()
        Check_Collison()

    End Sub
    Private Sub LoopMusic(sender As Object, e As EventArgs)
        BGMusic.Position = TimeSpan.Zero
        BGMusic.Play()
    End Sub
    Private Sub Check_Collison()

        Dim pt As Point = New Point(BALL_TRANSLATE.X, BALL_TRANSLATE.Y)
        VisualTreeHelper.HitTest(MainCanvas, Nothing, New HitTestResultCallback(AddressOf MyHitTestResult), New PointHitTestParameters(pt))


        If BALL_TRANSLATE.X <= WALL_LEFT And BALL_SPEED_X < 0 Then
            BALL_SPEED_X *= -1

        End If

        If BALL_TRANSLATE.X >= WALL_RIGHT And BALL_SPEED_X > 0 Then
            BALL_SPEED_X *= -1

        End If

        If BALL_TRANSLATE.Y < WALL_TOP And BALL_SPEED_Y < 0 Then
            BALL_SPEED_Y *= -1

        End If

        If BALL_TRANSLATE.Y > WALL_BOTTOM Then

            BALL_TRANSLATE.X = MainCanvas.Width / 2
            BALL_TRANSLATE.Y = MainCanvas.Height / 2

        End If

    End Sub
    Public Function MyHitTestResult(ByVal result As HitTestResult) As HitTestResultBehavior

        If result.VisualHit Is Paddle Then

            BALL_SPEED_Y *= -1

            Dim centerOfPaddleX As Double = PADDLE_TRANSLATE.X + CENTER_OF_PADDLE
            Dim ballDistFromPaddleCenterX As Double = BALL_TRANSLATE.X - centerOfPaddleX
            BALL_SPEED_X = ballDistFromPaddleCenterX * 0.08

            Return HitTestResultBehavior.Continue
        End If

        If TypeOf result.VisualHit Is Rectangle Then

            Dim brick As Rectangle = CType(result.VisualHit, Rectangle)

            If brick.Tag IsNot Nothing Then
                Dim health As Integer = CInt(brick.Tag)

                health -= 1
                FadeBrick(brick)

                If health <= 0 Then
                    MainCanvas.Children.Remove(brick)
                Else
                    brick.Tag = health

                    Select Case health
                        Case 2 : brick.Fill = Brushes.DeepSkyBlue
                        Case 1 : brick.Fill = Brushes.LightSkyBlue
                    End Select
                End If

                BALL_SPEED_Y *= -1
            End If
        End If

        Return HitTestResultBehavior.Continue
    End Function


    Private Sub DrawBricks()

        Dim rowColors As Brush() = {
        Brushes.CadetBlue,
        Brushes.Aqua,
        Brushes.Aquamarine,
        Brushes.Azure,
        Brushes.Blue,
        Brushes.Indigo,
        Brushes.Violet
    }

        For row = 0 To 6
            For columns = 0 To BRICK_COLS

                Dim BRICK As New Rectangle()
                BRICK.Height = BRICK_H - BRICK_GAP
                BRICK.Width = BRICK_W - BRICK_GAP
                BRICK.StrokeThickness = 2
                BRICK.RenderTransform = New TranslateTransform(BRICK_W * columns, BRICK_H * row)
                BRICK.Opacity = 1.0


                Dim health As Integer
                Select Case row
                    Case 0, 1 : health = 1
                    Case 2, 3 : health = 2
                    Case 4, 5 : health = 3
                    Case Else : health = 1
                End Select

                BRICK.Tag = health


                BRICK.Fill = rowColors(row)

                MainCanvas.Children.Add(BRICK)
            Next
        Next

    End Sub

    Private Sub FadeBrick(brick As Rectangle)
        Dim fade As New DoubleAnimation()
        fade.From = brick.Opacity
        fade.To = brick.Opacity - 0.1
        fade.Duration = TimeSpan.FromMilliseconds(150)

        brick.BeginAnimation(UIElement.OpacityProperty, fade)
    End Sub
    Private Sub DrawBall()
        With BALL
            .Fill = Brushes.Orange
            .Stroke = Brushes.Black
            .StrokeThickness = 2
            .Width = 20
            .Height = 20
            BALL.RenderTransform = BALL_TRANSLATE
        End With

        BALL_TRANSLATE.X = (MainCanvas.Width / 2)
        BALL_TRANSLATE.Y = (MainCanvas.Height / 2)
        MainCanvas.Children.Add(BALL)
    End Sub


    Private Sub MyWindow_KeyDown(sender As Object, e As KeyEventArgs) Handles MyWindow.KeyDown
        Select Case e.Key
            Case Key.A
                MOVE_LEFT = True
            Case Key.D
                MOVE_RIGHT = True
            Case Key.Escape
                Me.Close()
        End Select
    End Sub

    Private Sub MyWindow_KeyUp(sender As Object, e As KeyEventArgs) Handles MyWindow.KeyUp
        Select Case e.Key
            Case Key.A
                MOVE_LEFT = False
            Case Key.D
                MOVE_RIGHT = False
        End Select
    End Sub
    Private Sub SETWALLS()
        WALL_RIGHT = MainCanvas.Width - (BALL.Width + BALL_BUFFER)
        WALL_TOP += BALL.Height
        WALL_BOTTOM = MainCanvas.Height
    End Sub
    Private Sub MoveBall()
        BALL_TRANSLATE.X += BALL_SPEED_X
        BALL_TRANSLATE.Y += BALL_SPEED_Y
        BALL.RenderTransform = BALL_TRANSLATE
    End Sub
    Private Sub DrawPaddle()
        With Paddle
            .Fill = Brushes.Crimson
            .Stroke = Brushes.Black
            .StrokeThickness = 2
            .Width = 124
            .Height = 20
            Paddle.RenderTransform = PADDLE_TRANSLATE
            CENTER_OF_PADDLE = Paddle.Width / 2
        End With


        PADDLE_TRANSLATE.X = (MainCanvas.Width - Paddle.Width - PADDLE_BUFFER)
        PADDLE_TRANSLATE.Y = MainCanvas.Height - Paddle.Height - (PADDLE_DISTANCE_FROM_BOTTOM)

        MainCanvas.Children.Add(Paddle)
    End Sub
    Private Sub MovePaddle()
        If MOVE_LEFT Then
            PADDLE_TRANSLATE.X -= PADDLE_SPEED
        End If
        If MOVE_RIGHT Then
            PADDLE_TRANSLATE.X += PADDLE_SPEED
        End If
    End Sub



End Class
