
using Refit;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable annotations

namespace JobBank1111.Job.Contract
{
    /// <summary>My example API</summary>
    [System.CodeDom.Compiler.GeneratedCode("Refitter", "2.0.0.0")]
    public partial interface IMyexampleAPI
    {
        /// <remarks>
        /// This endpoint retrieves members using cursor pagination (v1).
        /// 
        /// Paging is controlled by request headers:
        /// - **x-page-size**: page size (default 10)
        /// - **x-next-page-token**: the token for the next page of results
        /// </remarks>
        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Accept: text/plain, application/json, text/json")]
        [Get("/api/v1/members:cursor")]
        Task<IApiResponse<GetMemberResponseCursorPaginatedList>> GetMemberCursorV1();

        /// <remarks>
        /// This endpoint retrieves members using offset pagination (v1).
        /// 
        /// Paging is controlled by request headers:
        /// - **x-page-index**: page index (default 0)
        /// - **x-page-size**: page size (default 10)
        /// - **cache-control**: set to `true` to bypass cache
        /// </remarks>
        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Accept: text/plain, application/json, text/json")]
        [Get("/api/v1/members:offset")]
        Task<IApiResponse<GetMemberResponsePaginatedList>> GetMemberOffsetV1();

        /// <remarks>Insert a new member (v1). Returns the inserted member.</remarks>
        /// <param name="body">body parameter</param>
        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Accept: application/json, text/json, text/plain", "Content-Type: application/json")]
        [Post("/api/v1/members")]
        Task<IApiResponse<InsertMemberResponse>> InsertMemberV1([Body] InsertMemberRequest body);

        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Accept: text/plain, application/json, text/json")]
        [Get("/api/v2/members:cursor")]
        Task<IApiResponse<GetMemberResponseCursorPaginatedList>> GetMembersCursor();

        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Accept: text/plain, application/json, text/json")]
        [Get("/api/v2/members:offset")]
        Task<IApiResponse<GetMemberResponsePaginatedList>> GetMemberOffset();

        /// <param name="body">body parameter</param>
        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Content-Type: application/json")]
        [Post("/api/v2/members")]
        Task<IApiResponse> InsertMember1([Body] InsertMemberRequest body);


    }

}

//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v14.7.1.0 (NJsonSchema v11.6.1.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
#pragma warning disable 612 // Disable "CS0612 '...' is obsolete"
#pragma warning disable 649 // Disable "CS0649 Field is never assigned to, and will always have its default value null"
#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."
#pragma warning disable 8073 // Disable "CS8073 The result of the expression is always 'false' since a value of type 'T' is never equal to 'null' of type 'T?'"
#pragma warning disable 3016 // Disable "CS3016 Arrays as attribute arguments is not CLS-compliant"
#pragma warning disable 8600 // Disable "CS8600 Converting null literal or possible null value to non-nullable type"
#pragma warning disable 8602 // Disable "CS8602 Dereference of a possibly null reference"
#pragma warning disable 8603 // Disable "CS8603 Possible null reference return"
#pragma warning disable 8604 // Disable "CS8604 Possible null reference argument for parameter"
#pragma warning disable 8625 // Disable "CS8625 Cannot convert null literal to non-nullable reference type"
#pragma warning disable 8765 // Disable "CS8765 Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes)."

namespace JobBank1111.Job.Contract
{
    using System = global::System;

    

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.7.1.0 (NJsonSchema v11.6.1.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class GetMemberResponse
    {

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("age")]
        public int? Age { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("sequenceId")]
        public long? SequenceId { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.7.1.0 (NJsonSchema v11.6.1.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class GetMemberResponseCursorPaginatedList
    {

        [JsonPropertyName("items")]
        public ICollection<GetMemberResponse> Items { get; set; }

        [JsonPropertyName("nextPageToken")]
        public string NextPageToken { get; set; }

        [JsonPropertyName("nextPreviousToken")]
        public string NextPreviousToken { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.7.1.0 (NJsonSchema v11.6.1.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class GetMemberResponsePaginatedList
    {

        [JsonPropertyName("items")]
        public ICollection<GetMemberResponse> Items { get; set; }

        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.7.1.0 (NJsonSchema v11.6.1.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class InsertMemberRequest
    {

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.7.1.0 (NJsonSchema v11.6.1.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class InsertMemberResponse
    {

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("age")]
        public int? Age { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("createdAt")]
        public System.DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("changedAt")]
        public System.DateTimeOffset? ChangedAt { get; set; }

        [JsonPropertyName("changedBy")]
        public string ChangedBy { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.7.1.0 (NJsonSchema v11.6.1.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class Failure
    {

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

    }


}

#pragma warning restore  108
#pragma warning restore  114
#pragma warning restore  472
#pragma warning restore  612
#pragma warning restore  649
#pragma warning restore 1573
#pragma warning restore 1591
#pragma warning restore 8073
#pragma warning restore 3016
#pragma warning restore 8600
#pragma warning restore 8602
#pragma warning restore 8603
#pragma warning restore 8604
#pragma warning restore 8625
#pragma warning restore 8765