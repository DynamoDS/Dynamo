$(document).ready(function(){
		$("td:contains('TypeParam :')").css("border-top","1px #939598 solid");
		$("td:contains('TypeParam :')").next().css("border-top","1px #939598 solid");		
		$(window).scroll(function() {
			var $myDiv = $('#menuNavigation');
			var st = $(this).scrollTop();
			var height = $myDiv.height();
			$myDiv.height(st );
        
		}).scroll();
	}
);