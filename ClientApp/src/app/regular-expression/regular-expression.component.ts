import { Component, OnInit, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Component({
  selector: "app-regular-expression",
  templateUrl: "./regular-expression.component.html",
  styleUrls: ["./regular-expression.component.css"]
})
export class RegularExpressionComponent implements OnInit {
  public Result: ExpressionOutput;
  public expression: string;
  public searchString: string;
  private _baseUrl: string;

  constructor(private http: HttpClient, @Inject("BASE_URL") baseUrl: string) {
    this._baseUrl = baseUrl;
    this.expression = "a*b";
    this.searchString = "appleandbanana";
  }

  ngOnInit() {
    var str = {
      expression: this.expression,
      searchString: this.searchString
    };
    this.http
      .post<ExpressionInput>(
        this._baseUrl + "RegularExpression/PostStatistics",
        str
      )
      .subscribe(
        result => {
          console.error(result);
        },
        error => console.error(error)
      );
  }
  evaluateExpression() {
    var json = JSON.stringify({
      Expression: this.expression,
      SearchString: this.searchString
    });
    this.http
      .get<ExpressionOutput>(
        this._baseUrl +
          "RegularExpression/GetStatistics" +
          "?regularExpression=" +
          encodeURIComponent(json)
      )
      .subscribe(
        result => {
          this.Result = result;
        },
        error => console.error(error)
      );
  }
  getALL() {
    var json = JSON.stringify({
      Expression: this.expression,
      SearchString: this.searchString
    });
    this.http
      .get<ExpressionOutput>(
        this._baseUrl +
          "RegularExpression/GetAll" +
          "?regularExpression=" +
          encodeURIComponent(json)
      )
      .subscribe(
        result => {
          this.Result = result;
        },
        error => console.error(error)
      );
  }
  getFirst() {
    var json = JSON.stringify({
      Expression: this.expression,
      SearchString: this.searchString
    });
    this.http
      .get<ExpressionOutput>(
        this._baseUrl +
          "RegularExpression/GetFirst" +
          "?regularExpression=" +
          encodeURIComponent(json)
      )
      .subscribe(
        result => {
          this.Result = result;
        },
        error => console.error(error)
      );
  }
  getNext() {
    var json = JSON.stringify({
      Expression: this.expression,
      SearchString: this.searchString
    });
    this.http
      .get<ExpressionOutput>(
        this._baseUrl +
          "RegularExpression/GetNext" +
          "?regularExpression=" +
          encodeURIComponent(json)
      )
      .subscribe(
        result => {
          this.Result = result;
        },
        error => console.error(error)
      );
  }
}

interface ExpressionInput {
  expression: string;
  searchString: string;
}
interface ExpressionOutput {
  outputText: string;
  errorText: string;
  completedSuccesfully: string;
  matchInfo: MatchInfo[];
}

interface MatchInfo {
  matchString: string;
  startIndex: number;
  endIndex: number;
  length: number;
}
