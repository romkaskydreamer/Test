import { IComparisonService } from "../services/interfaces";
import { ComparisonPostcode } from "../models/comparisonPostcode";
import { Industry } from "../models/industry";
import { Regions, ClaimIndustryStr, ClaimIndustry } from "../models/enums";

interface ICompareScope extends ng.IScope {
    isLoading: boolean;
    region: string;
    postcode: string;
    postcodes: ComparisonPostcode[];
    industries: Industry[];
    selectedIndustry: Industry;
    colours: string[];

    init(): void;
    totalCards(): number;
    totalTransactions(): number;
    totalMerchantSpend(): number;
    changeIndustry(industry: Industry): void;
    drawChart(selector: string, data: number[]): void;
    updateSvgs(): void;
    addPostcode(): void;
    updatePostcode(postcode: string, $index: number): void;
    removePostcode($index: number): void;
    canDisplay(postcode: ComparisonPostcode): boolean;
    renderPdf(): void;
    renderImage(): void;
}

class CompareController {
    private _minCards: number = 500;
    private _postcodeThreshold: number = 1000;

    constructor(
        private $scope: ICompareScope,
        private $state: ng.ui.IStateService,
        private comparisonService: IComparisonService,
        private $timeout: ng.ITimeoutService) {
        this.$scope.postcode = this.$state.params["postcode"];

        if (this.$state.params["region"] === Regions.Singapore.toString()) {
            this.$state.go("result", { postcode: this.$scope.postcode, region: Regions.Singapore.toString() });
        }
        this.$scope.region = this.$state.params["region"];

        // let inds = Object.keys(ClaimIndustry);
        // inds = inds.slice(inds.length / 2);
        // for (let ind of inds) {
        //    this.$scope.industries.push({ name: ConstIndustries[ind], value: inds.indexOf(ind) });
        // }

        // this.$scope.industries = <Industry[]>[
        //    {
        //        name: ClaimIndustryStr.All.toString(),
        //        value: ClaimIndustry.All
        //    },
        //    {
        //        name: ClaimIndustryStr.Retail.toString(),
        //        value: ClaimIndustry.Retail
        //    },
        //    {
        //        name: ClaimIndustryStr.Dining.toString(),
        //        value: ClaimIndustry.Dining
        //    },
        //    {
        //        name: ClaimIndustryStr.Lodge.toString(),
        //        value: ClaimIndustry.Lodge
        //    }];

        this.$scope.isLoading = true;
        this.$scope.industries = <Industry[]>[
            {
                name: "All",
                value: 0
            },
            {
                name: "Retail",
                value: 1
            },
            {
                name: "Dining",
                value: 2
            },
            {
                name: "Lodge",
                value: 3
        }];

        if (this.$scope.region === Regions.Australia.toString()) {
            this._postcodeThreshold = 0;
        }

        this.$scope.colours = [
            "#77206e",
            "#009aba",
            "#36c4b6",
            "#3d9c34",
            "#f1ae00",
            "#ec582a",
            "#898d8e",
            "#5e2750",
            "#006790",
            "#018566"
        ];

        this.$scope.init = () => {
            this.$scope.selectedIndustry = this.$scope.industries[0];
            this.$scope.postcodes = <ComparisonPostcode[]>[];

            let p: ComparisonPostcode = {
                postcode: this.$scope.postcode,
                cards: 0,
                merchantSpend: 0,
                transactions: 0,
                isLoading: false,
                errorMsg: "",
                threshold: 0
            };

            this.comparisonService
                .addPostcode(p);

            this.comparisonService
                .updatePostcode(p.postcode, 0, this.$scope.selectedIndustry.value, this.$scope.region)
                .then(a => this.$scope.updateSvgs());

            this.$scope.postcodes = this.comparisonService.postcodes;

            this.$scope.isLoading = false;
        };

        this.$scope.addPostcode = () => {
            let p: ComparisonPostcode = {
                postcode: "",
                cards: null,
                merchantSpend: null,
                transactions: null,
                isLoading: false,
                errorMsg: "",
                threshold: 0
            };

            this.comparisonService.addPostcode(p);
            this.$scope.postcodes = this.comparisonService.postcodes;
        };

        this.$scope.removePostcode = ($index: number) => {
            this.comparisonService.removePostcode($index);
            this.$scope.postcodes = this.comparisonService.postcodes;
            this.$scope.updateSvgs();
        };

        this.$scope.updatePostcode = (postcode: string, $index: number) => {

            this.comparisonService
                .updatePostcode(postcode, $index, this.$scope.selectedIndustry.value, this.$scope.region)
                .then(() => this.$scope.updateSvgs());
        };

        this.$scope.updateSvgs = () => {
            let cards = this.$scope.postcodes
                .filter(item => {
                    if (this.$scope.region === Regions.NewZealand.toString()) {
                        return item.cards > this._minCards || item.threshold > this._postcodeThreshold;
                    }
                    return item.cards > this._minCards;
                })
                .map(a => a.cards);

            let trans = this.$scope.postcodes
                .filter(item => {
                    if (this.$scope.region === Regions.NewZealand.toString()) {
                        return item.cards > this._minCards || item.threshold > this._postcodeThreshold;
                    }
                    return item.cards > this._minCards;
                })
                .map(a => a.transactions);

            let spend = this.$scope.postcodes
                .filter(item => {
                    if (this.$scope.region === Regions.NewZealand.toString()) {
                        return item.cards > this._minCards || item.threshold > this._postcodeThreshold;
                    }
                    return item.cards > this._minCards;
                })
                .map(item => item.merchantSpend);

            $("#cards").html("");
            $("#transactions").html("");
            $("#merchant").html("");

            this.$scope.drawChart("cards", cards);
            this.$scope.drawChart("transactions", trans);
            this.$scope.drawChart("merchant", spend);
        };

        this.$scope.drawChart = (selector: string, dataset: number[]) => {
            let width = 150;
            let height = 150;
            let radius = Math.min(width, height);

            let pie = d3.layout.pie().sort(null);

            let arc = d3.svg.arc();

            arc.outerRadius(radius - 80);

            let svg = d3.select("#" + selector)
                .append("svg")
                .attr("width", width)
                .attr("height", height)
                .attr("xmlns", "http://www.w3.org/2000/svg")
                .append("g")
                .attr("transform", "translate(" + width / 2 + "," + height / 2 + ")");

            let path = svg.selectAll("path")
                .data(pie(dataset))
                .enter()
                .append("path")
                .attr("fill",
                (d, i) => {
                    return this.$scope.colours[i];
                });
            path.attr("d", <any>arc);
        };

        this.$scope.totalCards = () => {
            let cards = this.$scope.postcodes
                .filter(item => {
                    if (this.$scope.region === Regions.NewZealand.toString()) {
                        return item.cards > this._minCards || item.threshold > this._postcodeThreshold;
                    }
                    return item.cards > this._minCards;
                })
                .map(a => a.cards);

            let total = 0;
            for (let i = 0; i < cards.length; i++) {
                total += cards[i];
            }
            return total;
        };

        this.$scope.totalTransactions = () => {
            let trans = this.$scope.postcodes
                .filter(item => {
                    if (this.$scope.region === Regions.NewZealand.toString()) {
                        return item.cards > this._minCards
                            || item.threshold > this._postcodeThreshold;
                    }
                    return item.cards > this._minCards;
                })
                .map(a => a.transactions);

            let total = 0;
            for (let i = 0; i < trans.length; i++) {
                total += trans[i];
            }
            return total;
        };

        this.$scope.totalMerchantSpend = () => {
            let spend = this.$scope.postcodes
                .filter(item => {
                    if (this.$scope.region === Regions.NewZealand.toString()) {
                        return item.cards > this._minCards
                            || item.threshold > this._postcodeThreshold;
                    }
                    return item.cards > this._minCards;
                })
                .map(a => a.merchantSpend);

            let total = 0;
            for (let i = 0; i < spend.length; i++) {
                total += spend[i];
            }
            return total;
        };

        this.$scope.changeIndustry = (industry: Industry) => {
            this.$scope.selectedIndustry = industry;

            this.$scope.postcodes.forEach((item, $index) => {
                this.$scope.updatePostcode(item.postcode, $index);
            });
        };

        this.$scope.renderPdf = () => {
            let cardSvg = $("#cards").html();
            let transSvg = $("#transactions").html();
            let spendSvg = $("#merchant").html();

            comparisonService.saveSvgs(cardSvg, transSvg, spendSvg)
                .then(results => {
                    let url = `/api/compare/pdf?industry=${this.$scope.selectedIndustry.value}&region=${this.$scope.region}`;
                    let validPostcodes = this.$scope.postcodes.filter(item => {
                        return item.postcode.length === 4;
                    });

                    validPostcodes.forEach(result => {
                        url += "&postcodes=" + result.postcode;
                    });

                    results.forEach(item => {
                        url += "&svgGuids=" + item;
                    });
                    window.location.href = url;
                });
        };

        this.$scope.renderImage = () => {
            let cardSvg = $("#cards").html();
            let transSvg = $("#transactions").html();
            let spendSvg = $("#merchant").html();

            comparisonService.saveSvgs(cardSvg, transSvg, spendSvg)
                .then(results => {
                    let url = `/api/compare/image?industry=${this.$scope.selectedIndustry.value}&region=${this.$scope.region}`;
                    let validPostcodes = this.$scope.postcodes.filter(item => {
                        return item.postcode.length === 4;
                    });

                    validPostcodes.forEach(result => {
                        url += "&postcodes=" + result.postcode;
                    });

                    results.forEach(item => {
                        url += "&svgGuids=" + item;
                    });
                    window.location.href = url;
                });
        };

        this.$scope.canDisplay = (postcode: ComparisonPostcode): boolean => {
            return (postcode.cards > this._minCards);
        };

        this.$scope.init();
    }
}

export { CompareController };
