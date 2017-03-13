import { StaticClaim } from "../models/staticClaim";
import { AutocompleteResult } from "../models/autocompleteResult";
import { PopulatedClaim } from "../models/populatedClaim";
import { ImageClaim } from "../models/Claim";
import { Source } from "../models/source";
import { ComparisonPostcode } from "../models/comparisonPostcode";
import { ClaimIndustry } from "../models/enums";

export interface ISearchService {
    autocomplete(query: string, region: string): ng.IPromise<AutocompleteResult[]>;
    isValidPostcode(query: string, region: string): ng.IPromise<boolean>;
}

export interface IClaimService {
    getPopulatedClaimsByPostcode(postcode: string, region: string): ng.IPromise<PopulatedClaim[]>;
    getImageClaims(region: string): ng.IPromise<ImageClaim[]>;
    getStaticClaims(region: string): ng.IPromise<StaticClaim[]>;
    getSources(region: string): ng.IPromise<Source[]>;
}

export interface IComparisonService {
    postcodes: ComparisonPostcode[];

    getComparisonPostcode(postcode: string, industry: ClaimIndustry, region: string): ng.IPromise<ComparisonPostcode>;

    saveSvgs(
        cardSvg: string,
        transSvg: string,
        spendSvg: string): ng.IPromise<string[]>;

    addPostcode(item: ComparisonPostcode);

    removePostcode(index: number);

    updatePostcode(
        postcode: string,
        index: number,
        selectedIndustry: ClaimIndustry, region: string): ng.IPromise<any>;
}
