import { ClaimIndustry, Type } from "./enums";

export class PopulatedClaim {
    public id: number;
    public claimName: string;
    public claimValue: string;
    public formattedClaimText: string;
    public industry: ClaimIndustry;
    public postcode: string;
    public threshold: number;
    public sourceId: number;
    public type: Type;
}
