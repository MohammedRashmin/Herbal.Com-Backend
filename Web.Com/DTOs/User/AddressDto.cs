using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.User;

public class CreateAddressDto
{
    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ZipCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;
}

public class UpdateAddressDto
{
    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ZipCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;
}

public class AddressResponseDto
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
