import { Component, OnInit, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ExpressionOutput } from "./expressionOutput";

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
      .post<ExpressionOutput>(
        this._baseUrl + "RegularExpression/PostStatistics",
        str
      )
      .subscribe(
        result => {
          this.Result = result;
          console.error(result);
        },
        error => console.error(error)
      );
  }
  evaluateExpression() {
    var str = {
      expression: this.expression,
      searchString: this.searchString
    };
    this.http
      .post<ExpressionOutput>(
        this._baseUrl + "RegularExpression/PostStatistics",
        str
      )
      .subscribe(
        result => {
          this.Result = result;
          console.error(result);
        },
        error => console.error(error)
      );
  }
  getAll() {
    this.http
      .get<ExpressionOutput>(this._baseUrl + "RegularExpression/GetAll")
      .subscribe(
        result => {
          this.Result = result;
        },
        error => console.error(error)
      );
  }
  getFirst() {
    this.http
      .get<ExpressionOutput>(this._baseUrl + "RegularExpression/GetFirst")
      .subscribe(
        result => {
          this.Result = result;
        },
        error => console.error(error)
      );
  }
  getNext() {
    this.http
      .get<ExpressionOutput>(this._baseUrl + "RegularExpression/GetNext")
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
