namespace SpeedyMailer.Bridge.Model.Drones
{
    public class MailDrone
    {
        public string WakeUpUri { get; set; }
        public DroneStatus Status { get; set; }
        public string BaseUri { get; set; }
        public int Id { get; set; }
    }
}