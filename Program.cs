﻿//
//    MCSkin3D, a 3d skin management studio for Minecraft
//    Copyright (C) 2013 Altered Softworks & MCSkin3D Team
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using MCSkin3D.ExceptionHandler;
using MCSkin3D.Languages;
using SVN;
using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Version = Paril.Components.Update.Version;

namespace MCSkin3D
{
	internal static class Program
	{
		public const string Name = "MCSkin3D LE";
		public static Version Version;
		public static MCSkin3DAppContext Context;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			AppDomain.CurrentDomain.AssemblyResolve +=
			(sender, args) =>
			{
				String resourceName = "MCSkin3D.Resources." + new AssemblyName(args.Name).Name + ".dll";

				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
				{
					if (stream == null)
						return null;

					var assemblyData = new Byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}
			};

			MainCore();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void MainCore()
		{
			MainThread = Thread.CurrentThread;

#if !DEBUG
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += Application_ThreadException;
#endif

			Version = new Version(Application.ProductVersion);
			Version.Revision = Repository.Revision;

#if !DEBUG
			try
			{
#endif
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MCSkin3DAppContext());
#if !DEBUG
			}
			catch (Exception ex)
			{
				RaiseException(ex);
			}
#endif
		}

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			RaiseException(e.Exception);
		}

		public static void RaiseException(Exception ex)
		{
			var raiseForm = Editor.MainForm.IsHandleCreated ? (Form)Editor.MainForm : (Form)Program.Context.SplashForm;

			if (raiseForm.InvokeRequired)
			{
				raiseForm.Invoke((Action)delegate() { RaiseException(ex); });
				return;
			}

			var form = new ExceptionForm();
			form.Exception = ex;

			if (Editor.CurrentLanguage == null)
				Editor.CurrentLanguage = Language.Parse(new StreamReader(new MemoryStream(Properties.Resources.English)));

			SystemSounds.Asterisk.Play();

			if (Editor.MainForm.Visible)
				form.ShowDialog(Editor.MainForm);
			else
				form.ShowDialog();
		}

		public static Thread MainThread { get; set; }
	}
}