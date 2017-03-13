import { IClaimService } from "./interfaces";
import { StaticClaim } from "../models/staticClaim";
import { PopulatedClaim } from "../models/populatedClaim";
import { ImageClaim } from "../models/Claim";
import { Source } from "../models/source";

class ClaimService implements IClaimService {
    public static inject = ["$http"];

    constructor(private $http: ng.IHttpService) { }

    public getPopulatedClaimsByPostcode = (postcode: string, region: string): ng.IPromise<PopulatedClaim[]> => {
        return this.$http.get(`api/claims/${region}/${postcode}`)
            .then(response => {
                return response.data;
            });
    }

    public getImageClaims = (region: string): ng.IPromise<ImageClaim[]> => {
        return this.$http.get(`api/claims/${region}/imageclaims`)
            .then(response => {
                return response.data;
            });
    }

    public getStaticClaims = (region: string): ng.IPromise<StaticClaim[]> => {
        return this.$http.get(`/api/claims/${region}/static`)
            .then(response => {
                return response.data;
            });
    }

    public getSources = (region: string): ng.IPromise<Source[]> => {
        return this.$http.get(`/api/claims/${region}/sources`)
            .then(response => {
                return response.data;
            });
    }
}

export { ClaimService };
