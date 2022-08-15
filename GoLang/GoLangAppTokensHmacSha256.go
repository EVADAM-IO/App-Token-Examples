package main

import (
	"./model"
	"bytes"
	"crypto/hmac"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"time"
	"github.com/google/uuid"
	"github.com/k0kubun/pp"
	"github.com/pkg/errors"
)

const URL = "https://api.evadam.com"
const EvadamAppToken = "6L6rqHEtRVvBKKt7P1A03k2x.h6OsEOXWpyaXAjvBVNnx3ccXNGTBLHkw" // Example: uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad
const EvadamSecretKey = "EraepapR4Grr2vI1eZWtTkFDhbhsC5EI"                             // Example: Hej2ch71kG2kTd1iIUDZFNsO5C1lh5Gq
//Please don't forget to change token and secret key values to production ones when switching to production

func main() {
	var externalUserId = uuid.NewString()

	var applicant = model.Applicant{}
	var fixedInfo = model.Info{}
	fixedInfo.Country = "TUN"
	fixedInfo.FirstName = "someName"
	applicant.FixedInfo = fixedInfo
	applicant.ExternalUserID = externalUserId

	applicant = CreateApplicant(applicant)

	applicant = GetApplicantInfo(applicant)

	fmt.Println(accessToken.Token)
}



func CreateApplicant(applicant model.Applicant) model.Applicant {
	postBody, _ := json.Marshal(applicant)

	b, err := _makeEvadamRequest(
		"/api/session/createSession",
		"POST",
		"application/json",
		postBody)
	if err != nil {
		log.Fatal(err)
	}
	pp.Println(string(b))
	ioutil.WriteFile("createApplicant.json", b, 0777)

	var ac model.Applicant
	err = json.Unmarshal(b, &ac)
	if err != nil {
		log.Fatal(err)
	}

	return ac
}

func GetApplicantInfo(applicant model.Applicant) model.Applicant {
	p := fmt.Sprintf("/api/applicants/%s/one", applicant.ID)
	b, err := _makeEvadamRequest(
		p,
		"GET",
		"application/json",
		nil)
	if err != nil {
		log.Fatal(err)
	}
	ioutil.WriteFile("getApplicant.json", b, 0777)

	var r model.Applicant
	err = json.Unmarshal(b, &r)
	if err != nil {
		log.Fatal(err)
	}
	pp.Println(r)

	return r
}

//X-App-Token - an App Token that you generate in our dashboard
//X-App-Access-Sig - signature of the request in the hex format (see below)
//X-App-Access-Ts - number of seconds since Unix Epoch in UTC
func _makeEvadamRequest(path, method, contentType string, body []byte) ([]byte, error) {

	request, err := http.NewRequest(method, URL+path, bytes.NewBuffer(body))
	if err != nil {
		return nil, err
	}

	ts := fmt.Sprintf("%d", time.Now().Unix())

	request.Header.Add("X-App-Token", EvadamAppToken)

	request.Header.Add("X-App-Access-Sig", _sign(ts, EvadamSecretKey, method, path, &body))
	request.Header.Add("X-App-Access-Ts", ts)
	request.Header.Add("Accept", "application/json")
	request.Header.Add("Content-Type", contentType)
	response, err := http.DefaultClient.Do(request)
	if err != nil {
		return nil, errors.WithStack(err)
	}
	defer response.Body.Close()

	b, err := ioutil.ReadAll(response.Body)
	if err != nil {
		return nil, errors.WithStack(err)
	}

	return b, nil
}

func _sign(ts string, secret string, method string, path string, body *[]byte) string {
	hash := hmac.New(sha256.New, []byte(secret))
	data := []byte(ts + method + path)

	if body != nil {
		data = append(data, *body...)
	}

	hash.Write(data)
	return hex.EncodeToString(hash.Sum(nil))
}
