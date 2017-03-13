import { Category } from "./enums";

export class StaticClaim {
    public id: number;
    public sourceId: number;
    public heading: string;
    public value: string;
    public claimText: string;
    public category: Category;
    public isRefreshing: boolean;
}
