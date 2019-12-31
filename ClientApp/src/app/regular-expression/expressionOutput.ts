import { MatchInfo } from "./matchInfo";

export interface ExpressionOutput {
  outputText: string;
  errorText: string;
  completedSuccesfully: string;
  matchInfo: MatchInfo[];
}
