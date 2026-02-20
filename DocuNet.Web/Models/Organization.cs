namespace DocuNet.Web.Models
{
    /// <summary>
    /// Representa uma entidade ou cliente que possui ativos e usuários associados.
    /// Funciona como um isolador de dados e permissões.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// Identificador único da organização.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nome da empresa ou organização.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Lista de dispositivos pertencentes a esta organização.
        /// </summary>
        public ICollection<Device> Devices { get; set; } = new List<Device>();

        /// <summary>
        /// Lista de usuários (membros) vinculados a esta organização.
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Define se a organização está ativa. Organizações inativas bloqueiam operações de seus membros.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
