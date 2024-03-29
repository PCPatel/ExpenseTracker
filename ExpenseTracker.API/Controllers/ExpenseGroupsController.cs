﻿using ExpenseTracker.Repository;
using ExpenseTracker.Repository.Factories;
using Marvin.JsonPatch;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace ExpenseTracker.API.Controllers
{
    public class ExpenseGroupsController : ApiController
    {
        IExpenseTrackerRepository _repository;
        ExpenseGroupFactory _expenseGroupFactory = new ExpenseGroupFactory();

        public ExpenseGroupsController()
        {
            _repository = new ExpenseTrackerEFRepository(new
                Repository.Entities.ExpenseTrackerContext());
        }

        public ExpenseGroupsController(IExpenseTrackerRepository repository)
        {
            _repository = repository;
        }


        public IHttpActionResult Get()
        {
            try
            {
                var expenseGroups = _repository.GetExpenseGroups();

                return Ok(expenseGroups.ToList()
                    .Select(eg => _expenseGroupFactory.CreateExpenseGroup(eg)));

            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        public IHttpActionResult Get(int id)
        {
            try
            {
                var expenseGroup = _repository.GetExpenseGroup(id);
                if (expenseGroup == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(_expenseGroupFactory.CreateExpenseGroup(expenseGroup));
                }
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody] DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }

                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                var result = _repository.InsertExpenseGroup(eg);
                if (result.Status == RepositoryActionStatus.Created)
                {
                    var newExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(result.Entity);
                    return Created(Request.RequestUri + "/" + newExpenseGroup.Id, newExpenseGroup);
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPut]
        public IHttpActionResult Put(int id, [FromBody] DTO.ExpenseGroup expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }

                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);
                var result = _repository.UpdateExpenseGroup(eg);
                if (result.Status == RepositoryActionStatus.Updated)
                {
                    var updatedExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(result.Entity);
                    return Ok(updatedExpenseGroup);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch(int id, [FromBody] JsonPatchDocument<DTO.ExpenseGroup> expenseGroupPatchDocument)
        {
            try
            {
                if (expenseGroupPatchDocument == null)
                {
                    return BadRequest();
                }

                var expenseGroup = _repository.GetExpenseGroup(id);
                if (expenseGroup == null)
                {
                    return NotFound();
                }

                var eg = _expenseGroupFactory.CreateExpenseGroup(expenseGroup);

                expenseGroupPatchDocument.ApplyTo(eg);

                var result = _repository.UpdateExpenseGroup(_expenseGroupFactory.CreateExpenseGroup(eg));

                if (result.Status == RepositoryActionStatus.Updated)
                {
                    var patchedExpenseGroup = _expenseGroupFactory.CreateExpenseGroup(result.Entity);

                    return Ok(patchedExpenseGroup);
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var result = _repository.DeleteExpenseGroup(id);

                if (result.Status == RepositoryActionStatus.Deleted)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }
                else if (result.Status == RepositoryActionStatus.NotFound)
                {
                    return NotFound();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}
