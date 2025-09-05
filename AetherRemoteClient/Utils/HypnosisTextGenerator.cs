using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.Utils;

public static class HypnosisTextGenerator
{
    // Screen bounding size, used to take a percentage of the whole screen
    private const float ScreenBoundingPercentage = 0.8f;
    
    // Clamp min and max font values
    private const float MinFontSize = 12;
    private const float MaxFontPercentage = 0.2f;
    
    // Iterations for binary search attempts
    private const int BinarySearchIterations = 10;

    /// <summary>
    ///     Compute the optimal size for texts to be displayed at
    /// </summary>
    public static async Task<List<HypnosisText>> ComputeTextSizes(ImFontPtr font, Vector2 screenSize, string[] texts)
    {
        var result = new List<HypnosisText>();
        foreach (var text in texts)
            result.Add(await Task.Run(() => ComputeTextSize(font, screenSize, text)).ConfigureAwait(false));
        
        return result;
    }
    
    /// <summary>
    ///     Compute the optimal size for this text to be displayed at
    /// </summary>
    private static HypnosisText ComputeTextSize(ImFontPtr font, Vector2 screenSize, string text)
    {
        // Prevent rendering at size zero, this throws an error
        if (screenSize.X is 0 || screenSize.Y is 0)
        {
            Plugin.Log.Warning("[HypnosisTextPreComputer.ComputeTextSizes] Screen size is zero, defaulting to monitor size.");
            screenSize = ImGui.GetIO().DisplaySize;
        }
        
        // Cap the maximum width and height proportional to the screen size
        var maxWidth = screenSize.X * ScreenBoundingPercentage;
        var maxHeight = screenSize.Y * ScreenBoundingPercentage;

        // Cap the max font size proportional to the screen size
        var maxFontSize = screenSize.Y * MaxFontPercentage;

        // Variables for discovering the best-fit font size
        var chosenSize = MinFontSize;
        var low = MinFontSize;
        var high = maxFontSize;

        // Iterate only 10 times to avoid too much work (10 x Entries)
        for (var i = 0; i < BinarySearchIterations; i++)
        {
            // Binary search method
            var middle = (low + high) * 0.5f;
            
            // Calculate the size
            var calculatedSize = ImGui.CalcTextSizeA(font, middle, float.MaxValue, maxWidth, text, out _);

            // Standard binary search, if it's lower than both, it's a valid font, but maybe still not the best fit
            if (calculatedSize.X <= maxWidth && calculatedSize.Y <= maxHeight)
            {
                chosenSize = middle;
                low = middle;
            }
            else
            {
                // It's too big, set this as our new high and continue
                high = middle;
            }
        }
        
        // Now we compute each line for breakpoints, and record the size so that the text can be centered when rendering
        var lines = new List<HypnosisLine>();
        
        // We will be chopping text off as we go, so copy it now
        var remainingText = text;

        // While there is still some text to process
        while (string.IsNullOrEmpty(remainingText) is false)
        {
            // Calculate the index in which a word wrap will occur. Note that we need to calculate the size relative to our font's original size
            var indexToWrapAt = ImGui.CalcWordWrapPositionA(font, chosenSize / font.FontSize, remainingText, maxWidth);

            // If there is no breakpoint
            if (indexToWrapAt <= 0 || indexToWrapAt >= remainingText.Length)
            {
                // Remove any text before or after
                var trimmed = remainingText.Trim();
                
                // Add the entire text with its size calculated (note we are not using a scale percentage here)
                lines.Add(new HypnosisLine(trimmed, ImGui.CalcTextSizeA(font, chosenSize, float.MaxValue, 0, trimmed, out _)));
                break;
            }

            // Otherwise, get the text up until the breakpoint
            var line = remainingText[..indexToWrapAt].Trim();
            
            // Add that text with its size calculated (note we are not using a scale percentage here)
            lines.Add(new HypnosisLine(line, ImGui.CalcTextSizeA(font, chosenSize, float.MaxValue, 0, line, out _)));
            
            // Remove all the text before the index and continue
            remainingText = remainingText[indexToWrapAt..].TrimEnd();
        }

        // Calculate the height of the entire word with wrapping included
        var height = 0f;
        foreach (var line in lines)
            height += line.Size.Y;

        // Return
        return new HypnosisText(chosenSize, new Vector2(0, height), lines);
    }
}



public record HypnosisText(float FontSize, Vector2 BoundingSize, List<HypnosisLine> Lines);
public record HypnosisLine(string Text, Vector2 Size);