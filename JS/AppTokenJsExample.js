const axios = require('axios');
const crypto = require('crypto');
const fs = require('fs');
const FormData = require('form-data');

// These parameters should be used for all requests
const EVADAM_APP_TOKEN = 'YOUR_EVADAM_APP_TOKEN'; // Example: uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad - Please don't forget to change when switching to production
const EVADAM_SECRET_KEY = 'YOUR_EVADAM_SECRET_KEY'; // Example: Hej2ch71kG2kTd1iIUDZFNsO5C1lh5Gq - Please don't forget to change when switching to production
const EVADAM_BASE_URL = 'https://api.evadam.com'; 

var config = {};
config.baseURL= EVADAM_BASE_URL;

axios.interceptors.request.use(createSignature, function (error) {
  return Promise.reject(error);
})

// This function creates signature for the request as described here: https://developers.EVADAM.com/api-reference/#app-tokens

function createSignature(config) {
  console.log('Creating a signature for the request...');

  var ts = Math.floor(Date.now() / 1000);
  const signature = crypto.createHmac('sha256',  EVADAM_SECRET_KEY);
  signature.update(ts + config.method.toUpperCase() + config.url);

  if (config.data instanceof FormData) {
    signature.update (config.data.getBuffer());
  } else if (config.data) {
    signature.update (config.data);
  }

  config.headers['X-App-Access-Ts'] = ts;
  config.headers['X-App-Access-Sig'] = signature.digest('hex');

  return config;
}

// These functions configure requests for specified method

// https://developers.EVADAM.com/api-reference/#creating-an-applicant
function createApplicant(externalUserId) {
  console.log("Creating an applicant...");

  var method = 'post';
  var url = '/api/session/createSession';
  var ts = Math.floor(Date.now() / 1000);
  
  var body = {
      externalUserId: externalUserId
  };

  var headers = {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      'X-App-Token': EVADAM_APP_TOKEN
  };

  config.method = method;
  config.url = url;
  config.headers = headers;
  config.data = JSON.stringify(body);

  return config;
}


// https://developers.EVADAM.com/api-reference/#getting-applicant-status-sdk
function getApplicantStatus(applicantId) {
  console.log("Getting the applicant status...");

  var method = 'get';
  var url = `/api/applicants/${applicantId}/`;

  var headers = {
    'Accept': 'application/json',
    'X-App-Token': EVADAM_APP_TOKEN
  };

  config.method = method;
  config.url = url;
  config.headers = headers;
  config.data = null;

  return config;
}


async function main() {
  externalUserId = "random-JSToken-" + Math.random().toString(36).substr(2, 9);
  console.log("External UserID: ", externalUserId); 

  response = await axios(createApplicant(externalUserId))
    .then(function (response) {
      console.log("Response:\n", response.data);
      return response;
    })
    .catch(function (error) {
      console.log("Error:\n", error.response.data);
    });
  
  const applicantId = response.data.id;
  console.log("ApplicantID: ", applicantId);


  response = await axios(getApplicantStatus(applicantId))
  .then(function (response) {
    console.log("Response:\n", response.data);
    return response;
  })
  .catch(function (error) {
    console.log("Error:\n", error.response.data);
  });


}

main();
