module Web.OpenCollective

open Thoth.Json 
open Fable

type Member = {
    MemberId:               int 
    CreatedAt:              string option
    Type:                   string option
    Role:                   string option
    IsActive:               bool option
    TotalAmountDonated:     float option
    Currency:               string option
    LastTransactionAt:      string option
    LastTransactionAmount:  float option
    Profile:                string option
    Name:                   string
    Company:                string option
    Description:            string option
    Image:                  string option
    Email:                  string option
    Twitter:                string option
    Github:                 string option
    Website:                string option
    }

module Member = 

    let decoder:string -> JsonValue -> Result<Member,DecoderError> =
        Decode.object (fun get ->
            {
                MemberId = get.Required.Field "MemberId" Decode.int
                CreatedAt = get.Optional.Field "createdAt" Decode.string
                Type = get.Optional.Field "type" Decode.string
                Role = get.Optional.Field "role" Decode.string
                IsActive = get.Optional.Field "isActive" Decode.bool
                TotalAmountDonated = get.Optional.Field "totalAmountDonated" Decode.float
                Currency = get.Optional.Field "currency" Decode.string
                LastTransactionAt = get.Optional.Field "lastTransactionAt" Decode.string
                LastTransactionAmount = get.Optional.Field "lastTransactionAmount" Decode.float
                Profile = get.Optional.Field "profile" Decode.string
                Name = get.Required.Field "name" Decode.string
                Company = get.Optional.Field "company" Decode.string
                Description = get.Optional.Field "description" Decode.string
                Image = get.Optional.Field "image" Decode.string
                // Email = get.Required.Field "email" Decode.option Decode.string 
                Email = get.Optional.Field "email" Decode.string
                Twitter = get.Optional.Field "twitter" Decode.string
                Github = get.Optional.Field "github" Decode.string
                Website = get.Optional.Field "website" Decode.string
            }
        )

module Memberlist = 
    let decoder:string -> JsonValue -> Result<list<Member>,DecoderError> = 
        Decode.list Member.decoder
    
let getSponsors (callback) = 
    let url = Literals.Urls.ApiEndpoints.OpenCollective
    let innerCallback (json: string) = 
        let r = Decode.fromString Memberlist.decoder json 
        match r with
        | Ok r -> callback r
        | Error e -> failwith e
    Helper.get(url, innerCallback)


