import random
import unittest
import autobuild as ab

class AutoBuildTests(unittest.TestCase):
        
    def setUp(self):
        self.test_clone = False # not performing clone test

    def test_setup_cleanup(self):
        path = 'ko'
        try:
            ab.setup(path)
            ab.cleanup(path)
        except Exception:
            self.fail("setup(path) raised ExceptionType unexpectedly!")

    def test_s3_publish(self):

        ab.publish_to_s3( 'tests', 'two' )

    def test_email_content(self):

        repo_date = "date"
        pull_result = "pull_result"
        result = "build_result"
        commits = "commits"
        errors = 5
        warnings = 5

        build_result_a = {'config': 'Release', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0)}
        build_result_b = {'config': 'Debug', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }
        
        build_results = [build_result_a, build_result_b]

        installer_result = "installer_result"

        ab.get_email_content( repo_date, pull_result, build_results, installer_result, commits, "unit test results" )

    def test_enumerate_tests(self):

        testfns = ab.enumerate_tests('tests')
        self.assertIn('Test2Tests.dll', testfns)
        self.assertIn('TestTests.dll', testfns)
        self.assertTrue(type(testfns) is str)
        self.assertEqual(testfns, "Test2Tests.dll TestTests.dll")
    
    def test_interpret_build(self):
        
        try:
            # failure
            result = open('tests/example_build_result_failure.txt', 'r').read()
            interpreted_res = ab.interpret_build( result )
            self.assertFalse( interpreted_res["success"] )

            #success
            result = open('tests/example_build_result_success.txt', 'r').read()
            interpreted_res = ab.interpret_build( result )
            self.assertTrue( interpreted_res["success"] )
        except Exception:
            self.fail("interpret_build raised Exception unexpectedly. Are you sure that the test files are in place?")
  
    def test_interpret_unit_tests(self):

        try:
            result = open('tests/example_nunit_result_failure.txt', 'r').read()
            interpreted_res = ab.interpret_unit_tests( result )
            self.assertFalse( interpreted_res["success"] )
        except Exception:
            self.fail("interpret_testd raised Exception unexpectedly. Are you sure that the test files are in place?")


    def test_log_results(self):

        repo_date = "date"
        pull_result = "pull_result"
        result = "build_result"
        errors = 5
        warnings = 5
        commits = "commits"
        log_prefix = ""

        build_result_a = {'config': 'Release', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0)}
        build_result_b = {'config': 'Debug', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }
        
        build_results = [build_result_a, build_result_b]

        installer_result = "installer_result"
        
        try:
            log = ab.log_results(log_prefix, pull_result, build_results, installer_result, commits, "unit test result")
            self.assertTrue( type(log) is str )
        except Exception:
            self.fail("log_result raised Exception unexpectedly!")

    def test_clone(self):

        if not self.test_clone:
            return

        local_repo = 'repo'
        repo_name = 'Dynamo'
        git_repo_path_https = 'https://github.com/ikeough/Dynamo.git'

        try:
            ab.clone( git_repo_path_https, local_repo, repo_name )
            ab.cleanup( ab.form_path(local_repo, repo_name) )
        except Exception:
            self.fail("clone raised ExceptionType unexpectedly!")


if __name__ == '__main__':
    unittest.main()