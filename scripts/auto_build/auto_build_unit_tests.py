import random
import unittest
import auto_build as ab

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

    def test_email_content(self):

        repo_date = "date"
        pull_result = "pull_result"
        result = "build_result"
        errors = 5
        warnings = 5

        build_result_a = {'config': 'Release', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0)}
        build_result_b = {'config': 'Debug', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }
        
        build_results = [build_result_a, build_result_b]

        installer_result = "installer_result"

        ab.get_email_content( repo_date, pull_result, build_results, installer_result )

    
    def test_interpret_build(self):
        
        #TOD): need mock build result
        result = """dynToolFinder.xaml.cs(40,48): warning CS0067: The event 'Dynamo.Elements.dynToolFinder.ToolFinderFinished' is never used [C:\Users\\boyerp\Desktop\dynamo_auto_build\\repos\\20130301T135228\Dynamo\Dynamo\DynamoElements.csproj]\r\n\r\n69 Warning(s)\r\n0 Error(s)\r\n\r\nTime Elapsed 00:00:05.65\r\n"""
        
        ab.interpret_build( result )
  
    def test_log_results(self):

        repo_date = "date"
        pull_result = "pull_result"
        result = "build_result"
        errors = 5
        warnings = 5
        log_prefix = ""

        build_result_a = {'config': 'Release', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0)}
        build_result_b = {'config': 'Debug', 'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }
        
        build_results = [build_result_a, build_result_b]

        installer_result = "installer_result"
        
        try:
            log = ab.log_results(log_prefix, pull_result, build_results, installer_result)
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