using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.Enum
{
	public enum StatusCode
	{
		OK = 200,
        InternalServerError = 500,
		NotFound = 404,

		IncorrectData = 405,

        //User
        IncorrectPassword = 1,
        PasswordsDontMatch = 2,
        UserNotFound = 4,
		UserExists = 7,
        EmailNotConfirmed = 8,

        //Entity
        EntityNotFound = 14,
		EntityExists = 17,

		//File
		FileExists = 27,

		//Payment
		IncorrectAmount = 33,
	}
}
