using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;


namespace UtilityLibrary
{
    public class MessageUtilities
    {
	    public static int defColumn { get; set; } = 35;

	    public static string nl = Environment.NewLine;

		public static int output { get; set; } = 0;

		public static RichTextBox rtb { get; set; }

		
		//
		//		private static string fmtInt(int int1)
		//		{
		//			return $"{int1,-4}";
		//		}

	    public static string fmtInt(int i)
	    {

		    return $"{i,-4}";
		}

		static public string fmt<T>(T var)
		{
			if (var is int)
			{
				return fmtInt(Convert.ToInt32(var));
			}

			return var.ToString();
		}

	    static public void logMsgFmtln(string msg1)
	    {
		    logMsgFmtln(msg1, "");
	    }

		static public void logMsgFmtln<T>(string msg1, T var1, int column = 0, int shift = 0)
		{
			logMsgFmt(msg1, fmt(var1), column, shift);
			logMsg(nl);
		}

		static public void logMsgFmt(string msg1)
		{
			logMsgFmt(msg1, "");
		}

		static public void logMsgFmt<T>(string msg1, T var1, int column = 0, int shift = 0)
		{
			logMsg(fmtMsg(msg1, fmt(var1), column, shift));
		}

		static public void logMsgFmt<T>(string msg1, T var1, Color color, Font font, int column = 0, int shift = 0)
		{
			logMsg(fmtMsg(msg1, fmt(var1), column, shift), color, font);
		}

		static public string fmtMsg<T>(string msg1, T var1, int column = 0, int shift = 0)
		{
			if (column == 0) { column = defColumn; }

			return string.Format(" ".Repeat(shift) + "{0," + column + "}{1}", msg1, fmt(var1));
		}

		static public void logMsgln<T1, T2>(T1 var1, T2 var2)
		{
			logMsg(fmt(var1));
			logMsgln(fmt(var2));
		}

		static public void logMsgln<T>(T var)
		{
			sendMsg(fmt(var));
			sendMsg(nl);
		}

		static public void logMsg<T1, T2>(T1 var1, T2 var2)
		{
			logMsg(fmt(var1));
			logMsg(fmt(var2));
		}

		static public void logMsg<T>(T var)
		{
			sendMsg(fmt(var));
		}

		static public void logMsg<T>(T var, Color color, Font font)
		{
			sendMsg(fmt(var), color, font);
		}

		static public void sendMsg(string msg, Color color = new Color(), Font font = null)
		{
			if (output == 0 && rtb != null)
			{
				rtb.AppendText(msg, color, font);
			}
			else
			{
				Console.Write(msg);
			}
		}

		static public void clearConsole()
		{
			DTE2 ide = (DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");

			OutputWindow w = ide.ToolWindows.OutputWindow;

			w.ActivePane.Activate();
			w.ActivePane.Clear();
		}
	
		static public string[] splitPath(string fileAndPath)
		{
			return fileAndPath.Split('\\');
		}
	}

	public static class StringExtensions
	{

		public static string Repeat(this string s, int quantity)
		{
			if (quantity <= 0) return "";

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < quantity; i++)
			{
				sb.Append(s);
			}

			return sb.ToString();
		}

		public static int CountSubstring(this string s, string substring)
		{
			int count = 0;
			int i = 0;
			while ((i = s.IndexOf(substring, i)) != -1)
			{
				i += substring.Length;
				count++;
			}

			return count;
		}

		public static int IndexOfToOccurance(this string s, string substring,
			int occurance, int start = 0)
		{
			if (s.Trim().Length == 0) { return -1; }
			if (occurance < 0) { return -1; }

			int pos = start;

			for (int count = 0; count < occurance; count++)
			{
				pos = s.IndexOf(substring, pos);

				if (pos == -1) { return pos; }

				pos += substring.Length;
			}
			return pos - substring.Length;
		}

		public static string GetSubDirectory(this string path, int requestedDepth)
		{
			requestedDepth++;
			if (requestedDepth == 0) { return "\\"; }

			path = path.TrimEnd('\\');

			int depth = path.CountSubstring("\\");

			if (requestedDepth > depth) { requestedDepth = depth; }

			int pos = path.IndexOfToOccurance("\\", requestedDepth);

			if (pos < 0) { return ""; }

			pos = path.IndexOfToOccurance("\\", requestedDepth + 1);

			if (pos < 0) { pos = path.Length; }

			return path.Substring(0, pos);
		}

		public static string GetSubDirectoryName(this string path, int requestedDepth)
		{
			requestedDepth++;

			path = path.TrimEnd('\\');

			if (requestedDepth > path.CountSubstring("\\")) { return ""; }

			string result = path.GetSubDirectory(requestedDepth - 1);

			if (result.Length == 0) { return ""; }

			int pos = path.IndexOfToOccurance("\\", requestedDepth) + 1;

			return result.Substring(pos);
		}

	}

	public static class RichTextBoxExtensions
	{
		public static void AppendText(this RichTextBox rtb, string text, Color color, Font font)
		{
			rtb.SuspendLayout();

			if (color != new Color() || font != null)
			{
				rtb.SelectionStart = rtb.TextLength;
				rtb.SelectionLength = 0;

				if (color != null)
				{
					rtb.SelectionColor = color;
				}
				if (font != null)
				{
					rtb.SelectionFont = font;
				}
				rtb.AppendText(text);
				rtb.SelectionColor = rtb.ForeColor;

			}
			else
			{
				rtb.AppendText(text);
			}

			rtb.ScrollToCaret();
			rtb.ResumeLayout();
		}
	}
}


