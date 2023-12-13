using AxMSTSCLib;
using MetroFramework.Forms;
using MSTSCLib;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace EYERemoteDeskptopApp
{
  public partial class Form1 : MetroForm
  {
    bool pingable = false;

    public Form1()
    {
      InitializeComponent();
      // Subscribe to the OnConnected event
      rdp.OnConnecting += rdp_OnConnected;

      // Subscribe to the OnDisconnected event
      rdp.OnDisconnected += rdp_OnDisconnected;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      try
      {
        btnDisconnect.Enabled = false;
        GetMyIp();
        GetWindowName();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error:" + ex.Message, "some Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void GetWindowName()
    {
      string MachineName1 = Environment.MachineName;
    }

    private void GetMyIp()
    {
      var host = Dns.GetHostEntry(Dns.GetHostName());

      foreach (var ip in host.AddressList)
      {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
          txtYourIp.Text = ip.ToString();
        }
      }
    }

    private void btnConnect_Click(object sender, EventArgs e)
    {
      try
      {
        string targetAddress = txtClientIp.Text.Trim();

        // Check if the input is a valid IP address
        if (!IPAddress.TryParse(targetAddress, out IPAddress ipAddress))
        {
          // If not a valid IP, try resolving it as a hostname
          try
          {
            ipAddress = Dns.GetHostAddresses(targetAddress)
                .FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);
          }
          catch (Exception ex)
          {
            MessageBox.Show($"Error resolving hostname: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }

          if (ipAddress == null)
          {
            MessageBox.Show("Invalid IP address or hostname", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }
        }

        // Now, ipAddress contains the valid IP address
        if (rdp.Connected == 0)
        {
          btnConnect.Enabled = false;
          btnDisconnect.Enabled = true;

          rdp.Server = ipAddress.ToString(); // Use the resolved IP address
          rdp.UserName = "Evince";
          rdp.AdvancedSettings9.EnableCredSspSupport = true;
          rdp.AdvancedSettings9.RDPPort = 3389;

          IMsTscNonScriptable Secured = (IMsTscNonScriptable)rdp.GetOcx();
          Secured.ClearTextPassword = txtPassword.Text;

          if (rdp.Connected == 1)
          {
            // Connection is already established, no need to connect again
            MessageBox.Show("Already connected.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
          else
          {
            // If not connected, attempt to connect
            rdp.Connect();
            //rdp.Visible= true;
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnDisconnect_Click(object sender, EventArgs e)
    {
      if (rdp.Connected == 1)
      {
        rdp.Disconnect();
      }

      btnConnect.Enabled = true;
      btnDisconnect.Enabled = false;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      // You can add timer-related logic here if needed
    }

    private void txtClientIp_Leave(object sender, EventArgs e)
    {
      string machineName = string.Empty;

      try
      {
        string ip = txtClientIp.Text;
        // MessageBox.Show(ip);
        System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(ip);
        machineName = hostEntry.HostName;

        lblClientsystemName.Text = "Client System Name" + " " + machineName;
      }
      catch (PingException)
      {
        // Handle PingException if needed
        throw;
      }
    }

    private void btnping_Click(object sender, EventArgs e)
    {
      try
      {
        Ping pinger = new Ping();
        string ipAddress = txtClientIp.Text;
        PingReply pingReply = pinger.Send(ipAddress);

        pingable = pingReply.Status == IPStatus.Success;

        if (pingable)
        {
          MessageBox.Show("Connection Success", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
          MessageBox.Show("Connection Timed Out", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      catch (Exception)
      {
        // Handle exceptions if needed
        throw;
      }
    }

    private void rdp_OnConnected(object sender, EventArgs e)
    {
      // Update UI or perform actions when connected
      MessageBox.Show("Connected successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

      // Add code here to display the client screen or perform any other actions
      // You can set the visibility of your remote desktop control or show another form, for example
      rdp.Visible = true;

      // Optionally, you can add more detailed logging
      Log("Connected.");
    }

    private void rdp_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
    {
      // Update UI or perform actions when disconnected
      btnConnect.Enabled = true;
      btnDisconnect.Enabled = false;

      // Optionally, you can add more detailed logging
      Log($"Disconnected. Reason: {e.discReason.ToString()}");

      MessageBox.Show($"Disconnected. Reason: {e.discReason.ToString()}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Log(string message)
    {
      // Add your logging mechanism here, such as writing to a log file or printing to the console
      Console.WriteLine(message);
      // Alternatively, you can use a logging library like NLog or log4net
    }


  }
}
