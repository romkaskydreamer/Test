import { IComparisonService } from "./interfaces";
import { ComparisonPostcode } from "../models/comparisonPostcode";
import { ClaimIndustry } from "../models/enums";

class ComparisonService implements IComparisonService {
    public static inject = [
        "$http",
        "$timeout",
        "$q"
    ];

    public postcodes: ComparisonPostcode[] = [];

    constructor(
        private $http: ng.IHttpService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService) {
    }

    public addPostcode(item: ComparisonPostcode) {
        if (this.postcodes.length < 10) {

            let existing = this.postcodes
                .filter(a => a.postcode === item.postcode);

            if (existing.length === 0) {
                this.postcodes.push(item);
            }
        }
    }

    public removePostcode(index: number) {
        this.postcodes.splice(index, 1);
    }

    public updatePostcode(
        postcode: string,
        index: number,
        selectedIndustry: ClaimIndustry, region: string): ng.IPromise<any> {

        let result: ng.IPromise<any> = null;
        let pc = this.postcodes[index];

        if (!pc) {
            return;
        }

        if (postcode.length >= 3) {
            pc.isLoading = true;

            let duplicatePostcodes = this.postcodes.filter(item => {
                return item.postcode === postcode && this.postcodes.indexOf(item) !== index;
            });

            if (duplicatePostcodes.length <= 0) {
                result = this.getComparisonPostcode(postcode, selectedIndustry, region)
                    .then((r: any) => {

                        this.postcodes[index] = r;
                        this.postcodes[index].isLoading = false;
                    });
            }
            else {
                // dupes are not allowed
                this.postcodes[index] = <ComparisonPostcode>{
                    postcode: "",
                    cards: null,
                    transactions: null,
                    merchantSpend: null,
                    errorMsg: "You cannot select the same postcode twice"
                };

                this.$timeout(() => {
                    this.postcodes[index].errorMsg = "";
                }, 2000);

                this.postcodes[index].isLoading = false;
            }
        }
        else {
            this.postcodes[index] = <ComparisonPostcode>{
                postcode,
                cards: null,
                transactions: null,
                merchantSpend: null
            };

            this.postcodes[index].isLoading = false;
        }

        if (!result) {
            let d = this.$q.defer();
            d.resolve();
            result = d.promise;
        }

        return result;
    }

    public getComparisonPostcode = (postcode: string, industry: ClaimIndustry, region: string) => {
        return this.$http.get(`/api/compare/${postcode}?industry=${industry}&region=${region}`)
            .then(response => {
                return response.data;
            });
    }

    public saveSvgs = (cardSvg: string, transSvg: string, spendSvg: string): ng.IPromise<string[]> => {
        return this.$http.post("/api/compare/savesvg", { cardSvg, transSvg, spendSvg }, {})
            .then(response => {
                return response.data;
            });
    }
}

export { ComparisonService };
