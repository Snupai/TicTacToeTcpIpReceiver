using static System.Console;
using System.Net.Sockets;
using System.Net;
using CommandLine;
using System.Text;
using System.ComponentModel.Design;
using static System.Net.Mime.MediaTypeNames;
using SkiaSharp;

namespace TicTacToeTcpIpReceiver
{
    internal class ShowOnDisplay
    {
        /// <summary>
        /// Draw a bitmap with grid lines and text using SkiaSharp library
        /// </summary>
        /// <param name="characters">A 2D array of characters to be displayed on the grid</param>
        public static void DrawBitmap(char[,] characters)
        {
            // Define canvas size
            int canvasWidth = 264;
            int canvasHeight = 176;

            // Create a new SKBitmap with white background
            using (var bitmap = new SKBitmap(canvasWidth, canvasHeight))
            {

                // Define paint for drawing bold lines
                using (var boldPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 2,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                })
                using (var canvas = new SKCanvas(bitmap))
                {
                    // Clear canvas with white color
                    canvas.Clear(SKColors.White);

                    // Define grid size and cell size
                    int gridSize = 3;
                    int cellSize = canvasHeight / gridSize;

                    // Define paint for drawing grid lines
                    using (var paint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = 1,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke
                    })
                    {
                        // Draw vertical grid lines
                        for (int i = 1; i < gridSize; i++)
                        {
                            float x = i * cellSize;
                            canvas.DrawLine(x, 0, x, canvasHeight - (gridSize), paint);
                        }

                        // Draw horizontal grid lines
                        for (int i = 1; i < gridSize; i++)
                        {
                            float y = i * cellSize;
                            canvas.DrawLine(0, y, cellSize * 3, y, paint);
                        }

                        // Draw bold vertical line to split the grids
                        canvas.DrawLine(cellSize * 3, 0, cellSize * 3, canvasHeight, boldPaint);
                    }

                    // Define paint for drawing characters
                    using (var textPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        TextSize = cellSize * 0.5f,
                        TextAlign = SKTextAlign.Center,
                        IsAntialias = true
                    })
                    {
                        // Draw characters in each cell
                        for (int row = 0; row < gridSize; row++)
                        {
                            for (int col = 0; col < gridSize; col++)
                            {
                                float x = col * cellSize + cellSize / 2;
                                float y = row * cellSize + cellSize / 2 + textPaint.TextSize / 3;

                                canvas.DrawText(characters[row, col].ToString(), x, y, textPaint);
                            }
                        }
                        // Draw Text on the empty right rotated by 90 degrees "ALF_Näher_Csharp"
                        textPaint.TextSize = cellSize * 0.3f;
                        textPaint.TextAlign = SKTextAlign.Left;
                        textPaint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                        canvas.RotateDegrees(90, cellSize * 3 + cellSize / 2, cellSize / 2);
                        canvas.DrawText("ALF_Näher_Csharp", cellSize * 3 + 6, 40, textPaint);

                        // draw another text "Tic Tac Toe"
                        canvas.DrawText("Tic Tac Toe", cellSize * 3 + 6, 10, textPaint);

                        // Fix rotate by -90 degrees
                        canvas.RotateDegrees(-90, cellSize * 3 + cellSize / 2, cellSize / 2);
                        //Draw Arrow alligned bottom right facing down which suggests this way is down
                        int bootomRightCornerX = canvasWidth - 20;
                        canvas.DrawLine(bootomRightCornerX - 10, cellSize * 3 - 10, bootomRightCornerX - 10, cellSize * 3 - 30, boldPaint);
                        canvas.DrawLine(bootomRightCornerX - 10, cellSize * 3 - 10, bootomRightCornerX - 20, cellSize * 3 - 20, boldPaint);
                        canvas.DrawLine(bootomRightCornerX - 10, cellSize * 3 - 10, bootomRightCornerX, cellSize * 3 - 20, boldPaint);

                        // Draw capital S above the Arrow
                        textPaint.TextSize = cellSize * 0.3f;
                        textPaint.TextAlign = SKTextAlign.Center;
                        canvas.DrawText("S", bootomRightCornerX - 10, cellSize * 3 - 30, textPaint);
                    }
                }


                // Load the bitmap into an SKImage and encode it as a PNG with quality 100
                using (var image = SKImage.FromBitmap(bitmap))
                // Encode the image data and save it to a file stream
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite("grid_image.png"))
                {
                    data.SaveTo(stream);
                }

                // Launch a Python script to draw on the e-paper with the generated image
                System.Diagnostics.Process.Start("python3.11", "draw_on_e-paper.py --image grid_image.png");
            }
        }
    }
}
