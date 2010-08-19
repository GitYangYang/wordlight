using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;

namespace WordLight
{
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private DTE2 _application;
        private AddIn _addInInstance;
        private WindowWatcher _watcher;

        public Connect()
        {
        }

        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _application = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            AddinSettings.Instance.Load(
                new SettingRepository(_application.Globals, "WordLight")
            );

            RegisterCommands();

            if (_watcher != null)
                _watcher.Dispose();
            _watcher = new WindowWatcher(_application);
        }

        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            _watcher.Dispose();
            if (disconnectMode == ext_DisconnectMode.ext_dm_HostShutdown || disconnectMode == ext_DisconnectMode.ext_dm_UserClosed)
            {
                RemoveCommands();
            }
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        private void RegisterCommands()
        {
            object[] contextGUIDS = new object[] { };
            Commands2 commands = (Commands2)_application.Commands;

            string toolsMenuName = GetLocalizedMenuName("Tools");

            //Place the command on the tools menu.
            //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            CommandBar menuBarCommandBar = ((CommandBars)_application.CommandBars)["MenuBar"];

            //Find the Tools command bar on the MenuBar command bar:
            CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
            CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

            //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
            //  just make sure you also update the QueryStatus/Exec method to include the new command names.
            try
            {
                //Add a command to the Commands collection:
                Command command = commands.AddNamedCommand2(_addInInstance, "Settings", "WordLight settings...", string.Empty, true, 102, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                //Add a control for the command to the tools menu:
                if ((command != null) && (toolsPopup != null))
                {
                    command.AddControl(toolsPopup.CommandBar, 1);
                }
            }
            catch (System.ArgumentException)
            {
                //If we are here, then the exception is probably because a command with that name
                //  already exists. If so there is no need to recreate the command and we can 
                //  safely ignore the exception.
            }
        }

        private void RemoveCommands()
        {
            List<Command> toDelete = new List<Command>();

            foreach (Command cmd in _application.Commands)
            {
                if (!string.IsNullOrEmpty(cmd.Name) && cmd.Name.StartsWith(_addInInstance.ProgID + "."))
                    toDelete.Add(cmd);
            }

            foreach (Command cmd in toDelete)
            {
                cmd.Delete();
            }
        }

        private string GetLocalizedMenuName(string menuName)
        {
            string localizedName;

            try
            {
                ResourceManager resourceManager =
                    new ResourceManager("TestAdding.CommandBar", Assembly.GetExecutingAssembly());
                CultureInfo cultureInfo = new CultureInfo(_application.LocaleID);

                string resourceName;

                if (cultureInfo.TwoLetterISOLanguageName == "zh")
                {
                    System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
                    resourceName = String.Concat(parentCultureInfo.Name, menuName);
                }
                else
                {
                    resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, menuName);
                }
                localizedName = resourceManager.GetString(resourceName);
            }
            catch
            {
                localizedName = "Tools";
            }

            return localizedName;
        }

        #region Implements IDTCommandTarget

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == _addInInstance.ProgID + ".Settings")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == _addInInstance.ProgID + ".Settings")
                {
                    handled = true;

                    using (Form dialog = new SettingsForm())
                    {
                        DialogResult result = dialog.ShowDialog();
                        if (result == DialogResult.OK)
                            AddinSettings.Instance.Save();
                        else
                            AddinSettings.Instance.Reload();
                    }

                    return;
                }
            }
        }

        #endregion
    }
}