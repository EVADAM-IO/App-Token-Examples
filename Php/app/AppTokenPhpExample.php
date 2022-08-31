<?php

namespace App;

require_once __DIR__ . '/../vendor/autoload.php';

use GuzzleHttp;
use GuzzleHttp\Psr7\MultipartStream;

define("EVADAM_SECRET_KEY", "EVADAM_SECRET_KEY"); // Example: Hej2ch71kG2kTd1iIUDZFNsO5C1lh5Gq
define("EVADAM_APP_TOKEN", "EVADAM_APP_TOKEN"); // Example: uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad
define("EVADAM_TEST_BASE_URL", "https://api.evadam.io");
//Please don't forget to change token and secret key values to production ones when switching to production

class AppTokenGuzzlePhpExample
{
    public function createApplicant($externalUserId)
    {
        $requestBody = [
            'externalUserId' => $externalUserId
            ];

        $url = '/api/session/createSession';
        $request = new GuzzleHttp\Psr7\Request('POST', EVADAM_TEST_BASE_URL . $url);
        $request = $request->withHeader('Content-Type', 'application/json');
        $request = $request->withBody(GuzzleHttp\Psr7\stream_for(json_encode($requestBody)));

        $responseBody = $this->sendHttpRequest($request, $url)->getBody();
        return json_decode($responseBody)->{'id'};
    }

    public function sendHttpRequest($request, $url)
    {
        $client = new GuzzleHttp\Client();
        $ts = time();

        $request = $request->withHeader('X-App-Token', EVADAM_APP_TOKEN);
        $request = $request->withHeader('X-App-Access-Sig', $this->createSignature($ts, $request->getMethod(), $url, $request->getBody()));
        $request = $request->withHeader('X-App-Access-Ts', $ts);
        
        // Reset stream offset to read body in `send` method from the start
        $request->getBody()->rewind();

        try {
            $response = $client->send($request);
            if ($response->getStatusCode() != 200 && $response->getStatusCode() != 201) {
                // If an unsuccessful answer is received, please log the value of the "correlationId" parameter.
                // Then perhaps you should throw the exception. (depends on the logic of your code)
            }
        } catch (GuzzleHttp\Exception\GuzzleException $e) {
            error_log($e);
        }

        return $response;
    }

    private function createSignature($ts, $httpMethod, $url)
    {
        return hash_hmac('sha256', $ts . strtoupper($httpMethod) . "/createSession" , EVADAM_SECRET_KEY);
    }

    public function getApplicantStatus($applicantId)
    {
        $url = "/api/applicants/" . $applicantId . "/";
        $request = new GuzzleHttp\Psr7\Request('GET', EVADAM_TEST_BASE_URL . $url);

        return $responseBody = $this->sendHttpRequest($request, $url)->getBody();
        return json_decode($responseBody);
    }

}


$externalUserId = uniqid();
$testObject = new AppTokenGuzzlePhpExample();

$applicantId = $testObject->createApplicant($externalUserId, $levelName);
echo "The applicant was successfully created: " . $applicantId . PHP_EOL

$applicantStatusStr = $testObject->getApplicantStatus($applicantId);
echo "Applicant status (json string): " . $applicantStatusStr;

?>
